using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class belial_controller : MonoBehaviour
{
    public GameObject pointStart;   //0
    public GameObject pointMiddle; // 1
    public GameObject pointEnd;    // 2

    private bool move = false;
    private bool back = true;
    private int currentPos = 0;
    private int rotationSpeed = 150;
    private float speed = 6;
    private Animator anim;
    private GameObject nextPoint;

    // --- Custom Cat Prefab / Model Settings ---
    [Header("Cat Model Settings")]
    public GameObject catModelPrefab;
    [Range(0.01f, 10f)] public float catModelScale = 0.65f; // Set default scale to 0.65f for perfect medium size
    public Vector3 catModelRotationOffset = Vector3.zero; // Default to Vector3.zero to face forward
    public Vector3 catModelTranslationOffset = new Vector3(0f, 0.55f, 0f); // Default translation Y to 0.55f

    // --- Cat Procedural Model References ---
    private Transform catRoot;
    private Transform catModelInstance;
    private Transform headGroup;
    private Transform legFL, legFR, legBL, legBR;
    private Transform tailGroup;
    private Transform bell;
    private float animTime = 0f;

    // --- Embedded Animation Clips & Playables ---
    private AnimationClip idleClip;
    private AnimationClip walkClip;
    private AnimationClip runClip;
    private AnimationClip jumpClip;
    
    private PlayableGraph playableGraph;
    private AnimationClip currentClip;

    private void Start()
    {
        anim = GetComponent<Animator>();
        nextPoint = pointMiddle;

        // Tự động sửa lại giá trị cũ đã bị Unity lưu (Serialized) trước đó để mèo có tỷ lệ cân đối hoàn hảo nhất
        if (catModelScale == 1.2f || catModelScale == 0.85f || catModelScale == 0.55f || catModelScale == 0.45f || catModelScale <= 0.05f) catModelScale = 0.65f;
        if (catModelTranslationOffset == Vector3.zero || catModelTranslationOffset == new Vector3(0f, 0.45f, 0f)) catModelTranslationOffset = new Vector3(0f, 0.55f, 0f);
        if (catModelRotationOffset == new Vector3(0f, 180f, 0f)) catModelRotationOffset = Vector3.zero;

        // Hide original robot meshes and disable original animator
        HideRobotModel();

        // Build or instantiate the cat model
        BuildProceduralCat();
    }

    private void Update()
    {
        // Periodic check to ensure robot meshes remain hidden
        HideRobotModel();

        if (move)
        {
            if (anim != null && anim.isActiveAndEnabled) anim.SetBool("walk", true);

            // Xác định hướng di chuyển mặc định theo từng chặng
            Vector3 normalDir = Vector3.zero;
            if (currentPos == 0){
                normalDir = new Vector3(1f, 0f, 0f);
                nextPoint = pointMiddle;
            }
            else if(currentPos == 1 && !back){
                normalDir = new Vector3(0f, 0f, 1f);
                nextPoint = pointEnd;
            }
            else if(currentPos == 1 && back){
                normalDir = new Vector3(-1f, 0f, 0f);
                nextPoint = pointStart;
            }
            else if(currentPos == 2){
                normalDir = new Vector3(0f, 0f, -1f);
                nextPoint = pointMiddle;
            }

            // Tính toán hướng di chuyển đã né ao cá bằng thuật toán Steering mềm mại
            Vector3 steerDir = GetSteeredDirection(transform.position, normalDir);

            // Xoay mèo mượt mà theo hướng di chuyển thực tế (đã né ao)
            if (steerDir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(steerDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
            }

            // Di chuyển mèo theo hướng né ao cá
            transform.position += steerDir * speed * Time.deltaTime;
            
            // Kiểm tra chuyển chặng di chuyển (Waypoint Transitions)
            if(transform.position.x >= pointMiddle.transform.position.x && !back && currentPos != 1)
            {
                currentPos = 1;               
            }  
            else if(transform.position.z <= pointMiddle.transform.position.z && back && currentPos != 1)
            {
                Debug.Log("IF");
                currentPos = 1;
            }
            else if(transform.position.z >= pointEnd.transform.position.z && !back)
            {
                move = false;
                if (anim != null && anim.isActiveAndEnabled) anim.SetBool("walk", false);
                currentPos = 2;
            }
            else if(transform.position.x <= pointStart.transform.position.x && back)
            {
                move = false;
                if (anim != null && anim.isActiveAndEnabled) anim.SetBool("walk", false);
                currentPos = 0;
            }
        }

        // Animate the cute procedural cat based on walking/idle states
        AnimateCat();

        // Ngăn chặn mèo chạy thẳng vào lòng hồ cá (Biện pháp bảo vệ vòng ngoài cứng)
        AvoidFishPonds();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!move)
            {
                move = true;
                back = !back;
            }
        }
    }

    private Vector3 GetSteeredDirection(Vector3 cPos, Vector3 normalDir)
    {
        FishPond[] ponds = FindObjectsByType<FishPond>(FindObjectsSortMode.None);
        if (ponds == null || ponds.Length == 0) return normalDir;

        FishPond closestPond = null;
        float closestDist = float.MaxValue;
        
        foreach (var pond in ponds)
        {
            if (pond == null) continue;
            float dist = Vector3.Distance(cPos, pond.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPond = pond;
            }
        }
        
        if (closestPond == null) return normalDir;
        
        Vector3 pCent = closestPond.transform.position;
        Vector3 toCat = cPos - pCent;
        toCat.y = 0f;
        float d2D = toCat.magnitude;
        
        // Tăng bán kính bắt đầu né lên pondRadius + 1.2m để mèo bắt đầu rẽ hướng từ xa, tránh sát sạt mép
        float avoidRadius = closestPond.pondRadius + 1.2f; 
        float minRadius = closestPond.pondRadius + 0.35f; // Giới hạn cứng
        
        if (d2D < avoidRadius)
        {
            Vector3 toCatNorm = toCat.normalized;
            if (toCatNorm == Vector3.zero) toCatNorm = Vector3.forward;
            
            // Tính toán 2 hướng tiếp tuyến trái và phải quanh ao cá
            Vector3 tangentCW = new Vector3(-toCatNorm.z, 0f, toCatNorm.x);
            Vector3 tangentCCW = new Vector3(toCatNorm.z, 0f, -toCatNorm.x);
            
            // Chọn hướng tiếp tuyến có độ tương đồng lớn hơn với hướng di chuyển gốc (để mèo đi tiếp thay vì quay đầu)
            float dotCW = Vector3.Dot(tangentCW, normalDir);
            float dotCCW = Vector3.Dot(tangentCCW, normalDir);
            Vector3 steerDir = (dotCW > dotCCW) ? tangentCW : tangentCCW;
            
            // Trộn hướng di chuyển gốc với hướng né ao cá
            float t = Mathf.InverseLerp(avoidRadius, minRadius, d2D); // 0 khi ở xa, 1 khi sát sạt bờ hồ
            
            // Trộn tiếp tuyến với hướng đẩy ra ngoài (outward push) để đảm bảo mèo không bị hút chéo vào bờ
            Vector3 targetSteer = Vector3.Slerp(steerDir, toCatNorm, 0.4f).normalized;
            
            return Vector3.Slerp(normalDir, targetSteer, t).normalized;
        }
        
        return normalDir;
    }

    private void AvoidFishPonds()
    {
        FishPond[] ponds = FindObjectsByType<FishPond>(FindObjectsSortMode.None);
        if (ponds == null || ponds.Length == 0) return;

        Vector3 cPos = transform.position;
        foreach (var pond in ponds)
        {
            if (pond == null) continue;
            Vector3 pCent = pond.transform.position;
            
            // Đo khoảng cách 2D trên mặt phẳng XZ
            Vector2 diff = new Vector2(cPos.x - pCent.x, cPos.z - pCent.z);
            float dist = diff.magnitude;
            
            // Bán kính lòng nước cần né tuyệt đối
            float minAvoidRadius = pond.pondRadius + 0.35f; 
            
            if (dist < minAvoidRadius)
            {
                Vector2 pushDir = diff.normalized;
                if (pushDir == Vector2.zero) pushDir = Vector2.up;
                
                // Đẩy vị trí của mèo ra ngoài rìa nước ao cá ngay lập tức
                Vector3 newPos = new Vector3(
                    pCent.x + pushDir.x * minAvoidRadius,
                    cPos.y,
                    pCent.z + pushDir.y * minAvoidRadius
                );
                transform.position = newPos;
                cPos = newPos;
            }
        }
    }

    // --- PROCEDURAL CAT BUILD & ANIMATION LOGIC ---

    private void HideRobotModel()
    {
        // Disable SkinnedMeshRenderers and MeshRenderers of the original robot companion
        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.enabled = false;
        }
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            // Do not disable our new procedural cat meshes
            if (mr.transform.IsChildOf(transform) && mr.transform != transform)
            {
                if (catRoot == null || !mr.transform.IsChildOf(catRoot))
                {
                    mr.enabled = false;
                }
            }
        }
        // Disable Animator component so it doesn't fight our procedural legs
        if (anim != null && anim.enabled) anim.enabled = false;
    }

    private void BuildProceduralCat()
    {
        // 1. Create Cat Root
        GameObject rootGO = new GameObject("Procedural_Cat");
        rootGO.transform.SetParent(transform, false);
        rootGO.transform.localPosition = Vector3.zero;
        rootGO.transform.localRotation = Quaternion.identity;
        catRoot = rootGO.transform;

        // 2. Try loading the custom GLB model uploaded by the user
        string path = "Assets/Prefabs/ModelVIp/Meshy_AI_3D_Cat_Model_Turnarou_0530085919_texture.glb";
#if UNITY_EDITOR
        if (catModelPrefab == null)
        {
            catModelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
#endif

        if (catModelPrefab != null)
        {
            GameObject inst = Instantiate(catModelPrefab, catRoot);
            inst.name = "Cat_Model_Instance";
            catModelInstance = inst.transform;
            catModelInstance.localPosition = catModelTranslationOffset;
            catModelInstance.localRotation = Quaternion.Euler(catModelRotationOffset);
            catModelInstance.localScale = Vector3.one * catModelScale;
            
            // In ra toàn bộ cấu trúc để kiểm tra xem có xương khớp (Rig/Armature) không
            Debug.Log("--- CẤU TRÚC MODEL MÈO GLB ---");
            int childCount = 0;
            foreach (Transform t in catModelInstance.GetComponentsInChildren<Transform>())
            {
                Debug.Log($"[Cat Structure] Child: {t.name}");
                childCount++;
            }
            Debug.Log($"Tổng số child transforms: {childCount}");
            
            // Try to load embedded animations from the GLB file
#if UNITY_EDITOR
            UnityEngine.Object[] subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in subAssets)
            {
                if (asset is AnimationClip)
                {
                    AnimationClip clip = (AnimationClip)asset;
                    string clipName = clip.name.ToLower();
                    if (clipName.Contains("idle")) idleClip = clip;
                    else if (clipName.Contains("walk")) walkClip = clip;
                    else if (clipName.Contains("run")) runClip = clip;
                    else if (clipName.Contains("jump")) jumpClip = clip;
                }
            }
#endif
            
            // Try to find sub-parts within the imported mesh model for advanced leg animation (fallback)
            FindModelParts(catModelInstance);
            Debug.Log("[belial_controller] Đã khởi tạo thành công mô hình mèo GLB và nạp hoạt ảnh đi kèm!");
        }
        else
        {
            // Fallback: Build our highly optimized, extremely cute cartoon 3D cat as a backup
            BuildBackupProceduralCat();
            Debug.LogWarning("[belial_controller] Không tìm thấy mô hình mèo GLB tại Assets/Prefabs/ModelVIp, đang sử dụng mô hình dự phòng!");
        }
    }

    private void FindModelParts(Transform modelRoot)
    {
        foreach (Transform t in modelRoot.GetComponentsInChildren<Transform>())
        {
            string name = t.name.ToLower();
            if (name.Contains("head")) headGroup = t;
            else if (name.Contains("tail")) tailGroup = t;
            else if (name.Contains("leg") && name.Contains("f") && name.Contains("l")) legFL = t;
            else if (name.Contains("leg") && name.Contains("f") && name.Contains("r")) legFR = t;
            else if (name.Contains("leg") && name.Contains("b") && name.Contains("l")) legBL = t;
            else if (name.Contains("leg") && name.Contains("b") && name.Contains("r")) legBR = t;
        }
    }

    private void BuildBackupProceduralCat()
    {
        // Create Materials using Helper or fallback URP/Standard
        Material orangeMat = ProceduralMeshHelper.CreateMaterial(new Color(0.95f, 0.52f, 0.15f));
        Material whiteMat  = ProceduralMeshHelper.CreateMaterial(new Color(0.98f, 0.98f, 0.98f));
        Material pinkMat   = ProceduralMeshHelper.CreateMaterial(new Color(0.98f, 0.62f, 0.68f));
        Material blackMat  = ProceduralMeshHelper.CreateMaterial(new Color(0.08f, 0.08f, 0.08f));
        Material redMat    = ProceduralMeshHelper.CreateMaterial(new Color(0.85f, 0.12f, 0.12f));
        Material goldMat   = ProceduralMeshHelper.CreateMaterial(new Color(0.95f, 0.78f, 0.18f));

        // Create Body (Orange Sphere)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(body.GetComponent<Collider>());
        body.name = "Cat_Body";
        body.transform.SetParent(catRoot, false);
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.4f, 0.75f);
        body.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // Chest/Belly (White spot)
        GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(belly.GetComponent<Collider>());
        belly.transform.SetParent(body.transform, false);
        belly.transform.localPosition = new Vector3(0f, -0.1f, 0.32f);
        belly.transform.localScale = new Vector3(0.85f, 0.75f, 0.6f);
        belly.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Collar (Red Ring)
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(collar.GetComponent<Collider>());
        collar.transform.SetParent(body.transform, false);
        collar.transform.localPosition = new Vector3(0f, 0.45f, 0.35f);
        collar.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
        collar.transform.localScale = new Vector3(0.65f, 0.08f, 0.65f);
        collar.GetComponent<MeshRenderer>().sharedMaterial = redMat;

        // Bell (Golden Sphere)
        GameObject bellGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(bellGO.GetComponent<Collider>());
        bellGO.transform.SetParent(collar.transform, false);
        bellGO.transform.localPosition = new Vector3(0f, -0.5f, 0.8f);
        bellGO.transform.localScale = new Vector3(0.28f, 2.2f, 0.28f);
        bellGO.GetComponent<MeshRenderer>().sharedMaterial = goldMat;
        bell = bellGO.transform;

        // Head Group
        GameObject headGroupGO = new GameObject("Head_Group");
        headGroupGO.transform.SetParent(body.transform, false);
        headGroupGO.transform.localPosition = new Vector3(0f, 0.65f, 0.42f);
        headGroup = headGroupGO.transform;

        // Head (Orange Sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(head.GetComponent<Collider>());
        head.transform.SetParent(headGroup, false);
        head.transform.localPosition = Vector3.zero;
        head.transform.localScale = new Vector3(1.05f, 0.95f, 0.95f);
        head.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // Muzzle
        GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(muzzle.GetComponent<Collider>());
        muzzle.transform.SetParent(head.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, -0.15f, 0.42f);
        muzzle.transform.localScale = new Vector3(0.55f, 0.32f, 0.35f);
        muzzle.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(nose.GetComponent<Collider>());
        nose.transform.SetParent(head.transform, false);
        nose.transform.localPosition = new Vector3(0f, -0.06f, 0.52f);
        nose.transform.localScale = new Vector3(0.12f, 0.08f, 0.08f);
        nose.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // Left Eye
        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(eyeL.GetComponent<Collider>());
        eyeL.transform.SetParent(head.transform, false);
        eyeL.transform.localPosition = new Vector3(-0.25f, 0.12f, 0.38f);
        eyeL.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeL.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

        GameObject highlightL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(highlightL.GetComponent<Collider>());
        highlightL.transform.SetParent(eyeL.transform, false);
        highlightL.transform.localPosition = new Vector3(-0.25f, 0.25f, 0.45f);
        highlightL.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        highlightL.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Right Eye
        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(eyeR.GetComponent<Collider>());
        eyeR.transform.SetParent(head.transform, false);
        eyeR.transform.localPosition = new Vector3(0.25f, 0.12f, 0.38f);
        eyeR.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeR.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

        GameObject highlightR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(highlightR.GetComponent<Collider>());
        highlightR.transform.SetParent(eyeR.transform, false);
        highlightR.transform.localPosition = new Vector3(0.25f, 0.25f, 0.45f);
        highlightR.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        highlightR.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Left Ear
        GameObject earL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(earL.GetComponent<Collider>());
        earL.transform.SetParent(head.transform, false);
        earL.transform.localPosition = new Vector3(-0.35f, 0.42f, 0f);
        earL.transform.localRotation = Quaternion.Euler(15f, 0f, 25f);
        earL.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        earL.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        GameObject innerEarL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(innerEarL.GetComponent<Collider>());
        innerEarL.transform.SetParent(earL.transform, false);
        innerEarL.transform.localPosition = new Vector3(0f, 0.05f, 0.45f);
        innerEarL.transform.localRotation = Quaternion.identity;
        innerEarL.transform.localScale = new Vector3(0.75f, 0.95f, 0.25f);
        innerEarL.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // Right Ear
        GameObject earR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(earR.GetComponent<Collider>());
        earR.transform.SetParent(head.transform, false);
        earR.transform.localPosition = new Vector3(0.35f, 0.42f, 0f);
        earR.transform.localRotation = Quaternion.Euler(15f, 0f, -25f);
        earR.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        earR.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        GameObject innerEarR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(innerEarR.GetComponent<Collider>());
        innerEarR.transform.SetParent(earR.transform, false);
        innerEarR.transform.localPosition = new Vector3(0f, 0.05f, 0.45f);
        innerEarR.transform.localRotation = Quaternion.identity;
        innerEarR.transform.localScale = new Vector3(0.75f, 0.95f, 0.25f);
        innerEarR.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // Legs
        // Leg FL
        GameObject legFL_GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(legFL_GO.GetComponent<Collider>());
        legFL_GO.transform.SetParent(body.transform, false);
        legFL_GO.transform.localPosition = new Vector3(-0.35f, -0.4f, 0.38f);
        legFL_GO.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f);
        legFL_GO.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
        legFL = legFL_GO.transform;

        GameObject pawFL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pawFL.GetComponent<Collider>());
        pawFL.transform.SetParent(legFL, false);
        pawFL.transform.localPosition = new Vector3(0f, -0.8f, 0.2f);
        pawFL.transform.localScale = new Vector3(1.2f, 0.35f, 1.4f);
        pawFL.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Leg FR
        GameObject legFR_GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(legFR_GO.GetComponent<Collider>());
        legFR_GO.transform.SetParent(body.transform, false);
        legFR_GO.transform.localPosition = new Vector3(0.35f, -0.4f, 0.38f);
        legFR_GO.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f);
        legFR_GO.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
        legFR = legFR_GO.transform;

        GameObject pawFR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pawFR.GetComponent<Collider>());
        pawFR.transform.SetParent(legFR, false);
        pawFR.transform.localPosition = new Vector3(0f, -0.8f, 0.2f);
        pawFR.transform.localScale = new Vector3(1.2f, 0.35f, 1.4f);
        pawFR.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Leg BL
        GameObject legBL_GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(legBL_GO.GetComponent<Collider>());
        legBL_GO.transform.SetParent(body.transform, false);
        legBL_GO.transform.localPosition = new Vector3(-0.35f, -0.4f, -0.38f);
        legBL_GO.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f);
        legBL_GO.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
        legBL = legBL_GO.transform;

        GameObject pawBL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pawBL.GetComponent<Collider>());
        pawBL.transform.SetParent(legBL, false);
        pawBL.transform.localPosition = new Vector3(0f, -0.8f, 0.2f);
        pawBL.transform.localScale = new Vector3(1.2f, 0.35f, 1.4f);
        pawBL.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Leg BR
        GameObject legBR_GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(legBR_GO.GetComponent<Collider>());
        legBR_GO.transform.SetParent(body.transform, false);
        legBR_GO.transform.localPosition = new Vector3(0.35f, -0.4f, -0.38f);
        legBR_GO.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f);
        legBR_GO.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
        legBR = legBR_GO.transform;

        GameObject pawBR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pawBR.GetComponent<Collider>());
        pawBR.transform.SetParent(legBR, false);
        pawBR.transform.localPosition = new Vector3(0f, -0.8f, 0.2f);
        pawBR.transform.localScale = new Vector3(1.2f, 0.35f, 1.4f);
        pawBR.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // Tail Group
        GameObject tailGroupGO = new GameObject("Tail_Group");
        tailGroupGO.transform.SetParent(body.transform, false);
        tailGroupGO.transform.localPosition = new Vector3(0f, 0.2f, -0.48f);
        tailGroup = tailGroupGO.transform;

        GameObject tail1 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Destroy(tail1.GetComponent<Collider>());
        tail1.transform.SetParent(tailGroup, false);
        tail1.transform.localPosition = new Vector3(0f, 0.2f, -0.1f);
        tail1.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
        tail1.transform.localScale = new Vector3(0.18f, 0.28f, 0.18f);
        tail1.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        GameObject tail2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Destroy(tail2.GetComponent<Collider>());
        tail2.transform.SetParent(tail1.transform, false);
        tail2.transform.localPosition = new Vector3(0f, 0.8f, 0.2f);
        tail2.transform.localRotation = Quaternion.Euler(40f, 0f, 0f);
        tail2.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        tail2.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        GameObject tailTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(tailTip.GetComponent<Collider>());
        tailTip.transform.SetParent(tail2.transform, false);
        tailTip.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        tailTip.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        tailTip.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;
    }

    private void PlayCatAnimation(AnimationClip clip)
    {
        if (clip == null || currentClip == clip) return;
        
        Animator catAnimator = catModelInstance != null ? catModelInstance.GetComponent<Animator>() : null;
        if (catAnimator == null)
        {
            if (catModelInstance != null) catAnimator = catModelInstance.gameObject.AddComponent<Animator>();
        }
        if (catAnimator == null) return;

        currentClip = clip;

        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }

        playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playableClip = AnimationClipPlayable.Create(playableGraph, clip);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "CatAnimation", catAnimator);
        playableOutput.SetSourcePlayable(playableClip);

        playableGraph.Play();
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }

    private void AnimateCat()
    {
        if (catRoot == null) return;

        animTime += Time.deltaTime;

        // Dynamically apply inspector adjustments at runtime (Editor real-time tweaking)
        if (catModelInstance != null)
        {
            catModelInstance.localPosition = catModelTranslationOffset;
            catModelInstance.localRotation = Quaternion.Euler(catModelRotationOffset);
        }

        // --- Ground Snapping Logic ---
        Vector3 parentPos = transform.position;
        Ray ray = new Ray(new Vector3(parentPos.x, parentPos.y + 15f, parentPos.z), Vector3.down);
        float groundY = parentPos.y; // Fallback
        
        RaycastHit[] hits = Physics.RaycastAll(ray, 45f);
        float bestGroundY = -999f;
        bool foundGround = false;
        
        foreach (var h in hits)
        {
            if (h.collider != null && !h.collider.CompareTag("Player") && !h.collider.isTrigger)
            {
                if (h.point.y > bestGroundY)
                {
                    bestGroundY = h.point.y;
                    foundGround = true;
                }
            }
        }
        
        if (foundGround)
        {
            groundY = bestGroundY;
        }
        
        float groundOffset = groundY - parentPos.y;

        bool hasClips = (idleClip != null || walkClip != null);
        bool hasLegs = (legFL != null && legFR != null);

        if (hasClips)
        {
            // --- SKELETAL GLB ANIMATIONS ---
            // Reset procedural offsets of catRoot so they don't fight the GLB armature skeleton
            catRoot.localRotation = Quaternion.identity;
            
            // Breathing animation only applied to scale (breathe scale) so the GLB skeleton moves legs naturally
            float breatheScaleY = 1.0f;
            float breatheScaleXZ = 1.0f;
            if (!move)
            {
                breatheScaleY = 1.0f + Mathf.Sin(animTime * 2.2f) * 0.012f;
                breatheScaleXZ = 1.0f / Mathf.Sqrt(breatheScaleY);
            }
            catRoot.localScale = new Vector3(breatheScaleXZ, breatheScaleY, breatheScaleXZ);
            catRoot.localPosition = new Vector3(0f, groundOffset, 0f);

            // Play the loaded GLB animation clips dynamically
            if (move)
            {
                if (walkClip != null) PlayCatAnimation(walkClip);
            }
            else
            {
                if (idleClip != null) PlayCatAnimation(idleClip);
            }
        }
        else
        {
            // --- FALLBACK PROCEDURAL ANIMATIONS ---
            if (move)
            {
                // --- WALKING STATE ---
                float walkSpeed = speed * 2.8f;
                float walkAngle = 32f;

                if (hasLegs)
                {
                    // 1. Swing legs alternatively
                    legFL.localRotation = Quaternion.Euler(Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
                    legFR.localRotation = Quaternion.Euler(-Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
                    if (legBL != null) legBL.localRotation = Quaternion.Euler(-Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
                    if (legBR != null) legBR.localRotation = Quaternion.Euler(Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);

                    // 2. Body bobbing + tilt
                    float bob = Mathf.Abs(Mathf.Sin(animTime * walkSpeed * 2f)) * 0.08f;
                    float tilt = Mathf.Sin(animTime * walkSpeed) * 3f;
                    catRoot.localPosition = new Vector3(0f, groundOffset + bob, 0f);
                    catRoot.localRotation = Quaternion.Euler(0f, 0f, tilt);
                    catRoot.localScale = Vector3.one;
                }
                else
                {
                    // No legs: Animate as a cute jumping/bouncing plush toy (Squash and Stretch!)
                    float bob = Mathf.Abs(Mathf.Sin(animTime * walkSpeed)) * 0.22f;
                    
                    float squashY = 1.0f - Mathf.Sin(animTime * walkSpeed * 2f) * 0.08f;
                    float stretchXZ = 1.0f / Mathf.Sqrt(squashY);
                    
                    float pitch = Mathf.Sin(animTime * walkSpeed) * 8f;
                    
                    catRoot.localPosition = new Vector3(0f, groundOffset + bob, 0f);
                    catRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                    catRoot.localScale = new Vector3(stretchXZ * catModelScale, squashY * catModelScale, stretchXZ * catModelScale);
                }

                // 3. Tail wagging
                if (tailGroup != null)
                {
                    float tailWag = Mathf.Sin(animTime * walkSpeed * 1.5f) * 20f;
                    tailGroup.localRotation = Quaternion.Euler(tailWag - 15f, tailWag, 0f);
                }

                // 4. Head bobbing
                if (headGroup != null)
                {
                    float headBob = Mathf.Sin(animTime * walkSpeed * 2f) * 5f;
                    headGroup.localRotation = Quaternion.Euler(headBob + 5f, 0f, 0f);
                }

                // 5. Bell jiggling
                if (bell != null)
                {
                    float bellJiggle = Mathf.Sin(animTime * walkSpeed * 3f) * 15f;
                    bell.localRotation = Quaternion.Euler(bellJiggle, 0f, bellJiggle);
                }
            }
            else
            {
                // --- IDLE STATE ---
                if (hasLegs)
                {
                    legFL.localRotation = Quaternion.Slerp(legFL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                    legFR.localRotation = Quaternion.Slerp(legFR.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                    if (legBL != null) legBL.localRotation = Quaternion.Slerp(legBL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                    if (legBR != null) legBR.localRotation = Quaternion.Slerp(legBR.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                }

                // Breathing animation (gentle scale/bobbing)
                float breathe = Mathf.Sin(animTime * 2.2f) * 0.015f;
                float breatheScaleY = 1.0f + Mathf.Sin(animTime * 2.2f) * 0.012f;
                float breatheScaleXZ = 1.0f / Mathf.Sqrt(breatheScaleY);

                catRoot.localPosition = new Vector3(0f, groundOffset + breathe, 0f);
                catRoot.localRotation = Quaternion.Slerp(catRoot.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                catRoot.localScale = new Vector3(breatheScaleXZ * catModelScale, breatheScaleY * catModelScale, breatheScaleXZ * catModelScale);

                // Tail slow wagging
                if (tailGroup != null)
                {
                    float tailWag = Mathf.Sin(animTime * 1.5f) * 12f;
                    tailGroup.localRotation = Quaternion.Euler(tailWag - 10f, tailWag * 0.6f, 0f);
                }

                // Head gentle tilt
                if (headGroup != null)
                {
                    float headTiltX = (Mathf.Sin(animTime * 0.8f) * 2.5f) + 2f;
                    float headTiltY = Mathf.Sin(animTime * 0.5f) * 6f;
                    headGroup.localRotation = Quaternion.Slerp(headGroup.localRotation, Quaternion.Euler(headTiltX, headTiltY, 0f), Time.deltaTime * 3f);
                }

                if (bell != null)
                {
                    bell.localRotation = Quaternion.Slerp(bell.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                }
            }
        }
    }
}
