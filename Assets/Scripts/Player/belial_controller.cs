using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    // --- Cat Procedural Model References ---
    private Transform catRoot;
    private Transform headGroup;
    private Transform legFL, legFR, legBL, legBR;
    private Transform tailGroup;
    private Transform bell;
    private float animTime = 0f;

    private void Start()
    {
        anim = GetComponent<Animator>();
        nextPoint = pointMiddle;

        // Hide original robot meshes and disable original animator
        HideRobotModel();

        // Build the cute cartoon 3D cat procedurally
        BuildProceduralCat();
    }

    private void Update()
    {
        // Periodic check to ensure robot meshes remain hidden
        HideRobotModel();

        if (move)
        {
            if (anim != null && anim.isActiveAndEnabled) anim.SetBool("walk", true);

            Vector3 direction = nextPoint.transform.position - transform.position;
            direction.y = 0f; // Keep rotation flat on ground
            if (direction != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
            }

            if (currentPos == 0){
                transform.position += new Vector3(speed, 0f, 0f) * Time.deltaTime;
                nextPoint = pointMiddle;
            }
            else if(currentPos == 1 && !back){
                transform.position += new Vector3(0f, 0f, speed) * Time.deltaTime;
                nextPoint = pointEnd;
            }
            else if(currentPos == 1 && back){
                transform.position += new Vector3(-speed, 0f, 0f) * Time.deltaTime;
                nextPoint = pointStart;
            }
            else if(currentPos == 2){
                transform.position += new Vector3(0f, 0f, -speed) * Time.deltaTime;
                nextPoint = pointMiddle;
            }
            
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

        // Create Materials using Helper or fallback URP/Standard
        Material orangeMat = ProceduralMeshHelper.CreateMaterial(new Color(0.95f, 0.52f, 0.15f));
        Material whiteMat  = ProceduralMeshHelper.CreateMaterial(new Color(0.98f, 0.98f, 0.98f));
        Material pinkMat   = ProceduralMeshHelper.CreateMaterial(new Color(0.98f, 0.62f, 0.68f));
        Material blackMat  = ProceduralMeshHelper.CreateMaterial(new Color(0.08f, 0.08f, 0.08f));
        Material redMat    = ProceduralMeshHelper.CreateMaterial(new Color(0.85f, 0.12f, 0.12f));
        Material goldMat   = ProceduralMeshHelper.CreateMaterial(new Color(0.95f, 0.78f, 0.18f));

        // 2. Create Body (Orange Sphere)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(body.GetComponent<Collider>());
        body.name = "Cat_Body";
        body.transform.SetParent(catRoot, false);
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.4f, 0.75f);
        body.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // 3. Chest/Belly (White spot)
        GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(belly.GetComponent<Collider>());
        belly.transform.SetParent(body.transform, false);
        belly.transform.localPosition = new Vector3(0f, -0.1f, 0.32f);
        belly.transform.localScale = new Vector3(0.85f, 0.75f, 0.6f);
        belly.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // 4. Collar (Red Ring)
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(collar.GetComponent<Collider>());
        collar.transform.SetParent(body.transform, false);
        collar.transform.localPosition = new Vector3(0f, 0.45f, 0.35f);
        collar.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
        collar.transform.localScale = new Vector3(0.65f, 0.08f, 0.65f);
        collar.GetComponent<MeshRenderer>().sharedMaterial = redMat;

        // 5. Bell (Golden Sphere)
        GameObject bellGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(bellGO.GetComponent<Collider>());
        bellGO.transform.SetParent(collar.transform, false);
        bellGO.transform.localPosition = new Vector3(0f, -0.5f, 0.8f);
        bellGO.transform.localScale = new Vector3(0.28f, 2.2f, 0.28f); // Scaled relative to collar
        bellGO.GetComponent<MeshRenderer>().sharedMaterial = goldMat;
        bell = bellGO.transform;

        // 6. Head Group (For bobbing/rotation)
        GameObject headGroupGO = new GameObject("Head_Group");
        headGroupGO.transform.SetParent(body.transform, false);
        headGroupGO.transform.localPosition = new Vector3(0f, 0.65f, 0.42f);
        headGroup = headGroupGO.transform;

        // 7. Head (Orange Sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(head.GetComponent<Collider>());
        head.transform.SetParent(headGroup, false);
        head.transform.localPosition = Vector3.zero;
        head.transform.localScale = new Vector3(1.05f, 0.95f, 0.95f); // Scale relative to body
        head.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // 8. Muzzle (White cheeks/snout)
        GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(muzzle.GetComponent<Collider>());
        muzzle.transform.SetParent(head.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, -0.15f, 0.42f);
        muzzle.transform.localScale = new Vector3(0.55f, 0.32f, 0.35f);
        muzzle.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // 9. Nose (Pink Sphere)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(nose.GetComponent<Collider>());
        nose.transform.SetParent(head.transform, false);
        nose.transform.localPosition = new Vector3(0f, -0.06f, 0.52f);
        nose.transform.localScale = new Vector3(0.12f, 0.08f, 0.08f);
        nose.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // 10. Left Eye (Large Black Sphere)
        GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(eyeL.GetComponent<Collider>());
        eyeL.transform.SetParent(head.transform, false);
        eyeL.transform.localPosition = new Vector3(-0.25f, 0.12f, 0.38f);
        eyeL.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeL.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

        // Left Eye Highlight (White Glossy Sphere)
        GameObject highlightL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(highlightL.GetComponent<Collider>());
        highlightL.transform.SetParent(eyeL.transform, false);
        highlightL.transform.localPosition = new Vector3(-0.25f, 0.25f, 0.45f);
        highlightL.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        highlightL.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // 11. Right Eye (Large Black Sphere)
        GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(eyeR.GetComponent<Collider>());
        eyeR.transform.SetParent(head.transform, false);
        eyeR.transform.localPosition = new Vector3(0.25f, 0.12f, 0.38f);
        eyeR.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        eyeR.GetComponent<MeshRenderer>().sharedMaterial = blackMat;

        // Right Eye Highlight (White Glossy Sphere)
        GameObject highlightR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(highlightR.GetComponent<Collider>());
        highlightR.transform.SetParent(eyeR.transform, false);
        highlightR.transform.localPosition = new Vector3(0.25f, 0.25f, 0.45f);
        highlightR.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        highlightR.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

        // 12. Left Ear (Orange Cylinder)
        GameObject earL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(earL.GetComponent<Collider>());
        earL.transform.SetParent(head.transform, false);
        earL.transform.localPosition = new Vector3(-0.35f, 0.42f, 0f);
        earL.transform.localRotation = Quaternion.Euler(15f, 0f, 25f);
        earL.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        earL.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // Left Inner Ear (Pink)
        GameObject innerEarL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(innerEarL.GetComponent<Collider>());
        innerEarL.transform.SetParent(earL.transform, false);
        innerEarL.transform.localPosition = new Vector3(0f, 0.05f, 0.45f);
        innerEarL.transform.localRotation = Quaternion.identity;
        innerEarL.transform.localScale = new Vector3(0.75f, 0.95f, 0.25f);
        innerEarL.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // 13. Right Ear (Orange Cylinder)
        GameObject earR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(earR.GetComponent<Collider>());
        earR.transform.SetParent(head.transform, false);
        earR.transform.localPosition = new Vector3(0.35f, 0.42f, 0f);
        earR.transform.localRotation = Quaternion.Euler(15f, 0f, -25f);
        earR.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        earR.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

        // Right Inner Ear (Pink)
        GameObject innerEarR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(innerEarR.GetComponent<Collider>());
        innerEarR.transform.SetParent(earR.transform, false);
        innerEarR.transform.localPosition = new Vector3(0f, 0.05f, 0.45f);
        innerEarR.transform.localRotation = Quaternion.identity;
        innerEarR.transform.localScale = new Vector3(0.75f, 0.95f, 0.25f);
        innerEarR.GetComponent<MeshRenderer>().sharedMaterial = pinkMat;

        // 14. Legs (4 Cylinders with White Paws)
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

        // 15. Tail Group (Curved Tail with White Tip)
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

    private void AnimateCat()
    {
        if (catRoot == null) return;

        animTime += Time.deltaTime;

        // --- Ground Snapping Logic ---
        Vector3 parentPos = transform.position;
        // Start the ray well above the parent to handle high terrain elevations, and cast down
        Ray ray = new Ray(new Vector3(parentPos.x, parentPos.y + 15f, parentPos.z), Vector3.down);
        float groundY = parentPos.y; // Fallback
        
        RaycastHit[] hits = Physics.RaycastAll(ray, 45f);
        float bestGroundY = -999f;
        bool foundGround = false;
        
        foreach (var h in hits)
        {
            if (h.collider != null && !h.collider.CompareTag("Player") && !h.collider.isTrigger)
            {
                // We want the highest solid ground under us
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

        if (move)
        {
            // --- WALKING STATE ---
            float walkSpeed = speed * 2.8f; // Cycle speed based on move speed
            float walkAngle = 32f;          // Swing range (degrees)

            // 1. Swing legs alternatively
            legFL.localRotation = Quaternion.Euler(Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
            legFR.localRotation = Quaternion.Euler(-Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
            legBL.localRotation = Quaternion.Euler(-Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);
            legBR.localRotation = Quaternion.Euler(Mathf.Sin(animTime * walkSpeed) * walkAngle, 0f, 0f);

            // 2. Body bobbing + tilt
            float bob = Mathf.Abs(Mathf.Sin(animTime * walkSpeed * 2f)) * 0.08f;
            float tilt = Mathf.Sin(animTime * walkSpeed) * 3f;
            catRoot.transform.localPosition = new Vector3(0f, groundOffset + bob, 0f);
            catRoot.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

            // 3. Tail wagging widely
            float tailWag = Mathf.Sin(animTime * walkSpeed * 1.5f) * 20f;
            tailGroup.localRotation = Quaternion.Euler(tailWag - 15f, tailWag, 0f);

            // 4. Head bobbing
            float headBob = Mathf.Sin(animTime * walkSpeed * 2f) * 5f;
            headGroup.localRotation = Quaternion.Euler(headBob + 5f, 0f, 0f);

            // 5. Bell jiggling
            float bellJiggle = Mathf.Sin(animTime * walkSpeed * 3f) * 15f;
            bell.localRotation = Quaternion.Euler(bellJiggle, 0f, bellJiggle);
        }
        else
        {
            // --- IDLE STATE ---
            // 1. Reset legs to vertical
            legFL.localRotation = Quaternion.Slerp(legFL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            legFR.localRotation = Quaternion.Slerp(legFR.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            legBL.localRotation = Quaternion.Slerp(legBL.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            legBR.localRotation = Quaternion.Slerp(legBR.localRotation, Quaternion.identity, Time.deltaTime * 5f);

            // 2. Breathing animation (gentle scale/bobbing)
            float breathe = Mathf.Sin(animTime * 2.2f) * 0.015f;
            catRoot.transform.localPosition = new Vector3(0f, groundOffset + breathe, 0f);
            catRoot.transform.localRotation = Quaternion.Slerp(catRoot.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);

            // 3. Slow tail wagging
            float tailWag = Mathf.Sin(animTime * 1.5f) * 12f;
            tailGroup.localRotation = Quaternion.Euler(tailWag - 10f, tailWag * 0.6f, 0f);

            // 4. Gentle head tilt (idle curiosity)
            float headTiltX = (Mathf.Sin(animTime * 0.8f) * 2.5f) + 2f;
            float headTiltY = Mathf.Sin(animTime * 0.5f) * 6f;
            headGroup.localRotation = Quaternion.Slerp(headGroup.localRotation, Quaternion.Euler(headTiltX, headTiltY, 0f), Time.deltaTime * 3f);

            // 5. Bell gentle jiggle
            bell.localRotation = Quaternion.Slerp(bell.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }
}
