﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Vectrosity;

struct Pose
{
    public Vector3 position;
    public Quaternion rotate;
    public Pose(Vector3 v, Quaternion r)
    {
        position = v;
        rotate = r;
    }
}

public class TraceBone : MonoBehaviour {
    /*
     * hip 
     *     -leftup_leg
     *          -left_leg
     *     -rightup_leg
     *          -right_leg
     *     -spine
     *          -left_shoulder
     *              -left_arm
     *                  -leftfore_arm
     *          -right_shoulder
     *              -right_arm
     *                  -rightfore_arm
     *          -neck
    */
    Transform hip;
    Transform leftup_leg;
    Transform left_leg;
    Transform rightup_leg;
    Transform right_leg;
    Transform left_foot;
    Transform right_foot;
    Transform spine;
    Transform left_shoulder;
    Transform left_arm;
    Transform leftfore_arm;
    Transform right_shoulder;
    Transform right_arm;
    Transform rightfore_arm;
    Transform left_hand;
    Transform right_hand;
    Transform neck;
    Transform[] mirror_body;
    List<Vector3> root_motion_trace;
    List<Pose> hip_trace;
    List<Pose> leftup_leg_trace;
    List<Pose> left_leg_trace;
    List<Pose> rightup_leg_trace;
    List<Pose> right_leg_trace;
    List<Pose> spine_trace;
    List<Pose> left_arm_trace;
    List<Pose> leftfore_arm_trace;
    List<Pose> right_arm_trace;
    List<Pose> rightfore_arm_trace;
    List<Pose> neck_trace;
    int trace_frame;
    float height;
    Animator anim;
    enum TraceBehavior{
        TraceRun,
        TraceJump,
        TraceRoll
    };
    TraceBehavior trace_behavior = TraceBehavior.TraceRoll;
    Transform GetTransform(Transform check, string name)
    {
        foreach (Transform t in check.GetComponentsInChildren<Transform>())
        {
            Transform ret;
            if (t == check)
                continue;
            if (t.name == name) { return t; }
            ret = GetTransform(t, name);
            if (ret !=null)
                return ret;
        }
        return null;
    }

	// Use this for initialization
	void Awake () {
        hip = GetTransform(transform, "char_robotGuard_Hips");
        leftup_leg = GetTransform(hip, "char_robotGuard_LeftUpLeg");
        left_leg = GetTransform(leftup_leg, "char_robotGuard_LeftLeg");
        rightup_leg = GetTransform(hip, "char_robotGuard_RightUpLeg");
        right_leg = GetTransform(rightup_leg, "char_robotGuard_RightLeg");
        left_foot = GetTransform(left_leg, "char_robotGuard_LeftFoot");
        right_foot = GetTransform(right_leg, "char_robotGuard_RightFoot");
        spine = GetTransform(hip, "char_robotGuard_Spine");
        left_shoulder = GetTransform(spine, "char_robotGuard_LeftShoulder");
        left_arm = GetTransform(left_shoulder, "char_robotGuard_LeftArm");
        leftfore_arm = GetTransform(left_arm, "char_robotGuard_LeftForeArm");
        right_shoulder = GetTransform(spine, "char_robotGuard_RightShoulder");
        right_arm = GetTransform(right_shoulder, "char_robotGuard_RightArm");
        rightfore_arm = GetTransform(right_arm, "char_robotGuard_RightForeArm");
        left_hand = GetTransform(leftfore_arm, "char_robotGuard_LeftHand");
        right_hand = GetTransform(rightfore_arm, "char_robotGuard_RightHand");
        neck = GetTransform(spine, "char_robotGuard_Neck");
        anim = GetComponent<Animator>();     
        
        if (hip == null)
            Debug.Log("hip not found");
        if (leftup_leg==null || left_leg==null)
            Debug.Log("left leg not found");
        if (rightup_leg == null || right_leg == null)
            Debug.Log("right leg not found");
        if (left_arm == null || leftfore_arm == null)
            Debug.Log("left arm not found");
        if (right_arm == null || rightfore_arm == null)
            Debug.Log("right arm not found");
        if (spine == null || neck == null)
            Debug.Log("neck not found");
        if (left_foot == null || right_foot == null)
            Debug.Log("foot not found");
        if (left_hand == null || right_hand == null)
            Debug.Log("hand not found");
        Vector3 knee = (left_leg.position + right_leg.position) / 2;
        height = (neck.position - knee).y;
        Debug.Log("hight=" + height);

        hip_trace = new List<Pose>();
        leftup_leg_trace = new List<Pose>();
        left_leg_trace = new List<Pose>();
        rightup_leg_trace = new List<Pose>();
        right_leg_trace = new List<Pose>();
        spine_trace = new List<Pose>();
        left_arm_trace = new List<Pose>();
        leftfore_arm_trace = new List<Pose>();
        right_arm_trace = new List<Pose>();
        rightfore_arm_trace = new List<Pose>();
        neck_trace = new List<Pose>();
        root_motion_trace = new List<Vector3>();
        float err = 0;
        err += Vector3.Magnitude(hip.rotation * leftup_leg.localPosition + hip.position - leftup_leg.position);        
        err += Vector3.Magnitude(hip.rotation * leftup_leg.localPosition + (hip.localRotation *leftup_leg.localRotation)*left_leg.localPosition + hip.position - left_leg.position);
        err += Vector3.Magnitude(((hip.rotation * rightup_leg.localPosition + hip.position) - rightup_leg.position));
        err += Vector3.Magnitude(hip.rotation * (rightup_leg.localPosition + rightup_leg.localRotation * right_leg.localPosition) + hip.position - right_leg.position);
        err += Vector3.Magnitude(hip.rotation * spine.localRotation * (left_shoulder.localPosition + left_shoulder.localRotation * left_arm.localPosition) + spine.position - left_arm.position);
        err += Vector3.Magnitude(spine.rotation * (right_shoulder.localPosition + right_shoulder.localRotation * right_arm.localPosition) + spine.position - right_arm.position);
        err += Vector3.Magnitude(spine.rotation * (right_shoulder.localPosition + right_shoulder.localRotation * right_arm.localPosition + (right_shoulder.localRotation * right_arm.localRotation) * rightfore_arm.localPosition) + spine.position - rightfore_arm.position);
        err += Vector3.Magnitude(spine.rotation * left_shoulder.localPosition + left_shoulder.rotation * left_arm.localPosition + left_arm.rotation * leftfore_arm.localPosition + spine.position - leftfore_arm.position);
        Debug.Log("err=" + err);
        trace_frame = 0;

        Vector3[] position = new Vector3[GenerateBone.TOTAL_PART];
        position[GenerateBone.HIP] = hip.position;
        position[GenerateBone.RIGHTUP_LEG] = rightup_leg.position;
        position[GenerateBone.LEFTUP_LEG] = leftup_leg.position;
        position[GenerateBone.RIGHT_LEG] = right_leg.position;
        position[GenerateBone.LEFT_LEG] = left_leg.position;
        position[GenerateBone.RIGHT_FOOT] = right_foot.position;
        position[GenerateBone.LEFT_FOOT] = left_foot.position;

        position[GenerateBone.SPINE] = spine.position;
        position[GenerateBone.RIGHT_ARM] = right_arm.position;
        position[GenerateBone.RIGHTFORE_ARM] = rightfore_arm.position;
        position[GenerateBone.RIGHT_HAND] = right_hand.position;
        position[GenerateBone.LEFT_ARM] = left_arm.position;
        position[GenerateBone.LEFTFORE_ARM] = leftfore_arm.position;
        position[GenerateBone.LEFT_HAND] = left_hand.position;
        position[GenerateBone.NECK] = neck.position;

        Vector3[] normal_pos;
        Quaternion[] normal_rot;
        GenerateBone.compute_normal(position, out normal_pos, out normal_rot);
        GenerateBone.generate_bone(normal_pos, 0.05f, out mirror_body, true);
        mirror_body[GenerateBone.HIP].parent.position = new Vector3(1, 1, 0);
        /*
        GenerateBone.from_to_zx(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        for (int i = 0; i < 1000; i++)
        {
            float a0 = Random.Range(0, Mathf.PI/2);
            float a1 = Random.Range(0, Mathf.PI*2);
            float a2 = Random.Range(0, Mathf.PI*2);
            Quaternion r = new Quaternion(Mathf.Sin(a0) * Mathf.Sin(a1) * Mathf.Sin(a2),
                                            Mathf.Sin(a0) * Mathf.Sin(a1) * Mathf.Cos(a2),
                                            Mathf.Sin(a0) * Mathf.Cos(a1), Mathf.Cos(a0));
            Vector3 f0, f1;
            Quaternion ret;
            if (true)
            {
                f0 = new Vector3(Random.value, Random.value, Random.value);
                f1 = new Vector3(Random.value, Random.value, Random.value);
                ret = GenerateBone.from_to(f0, f1, r * f0, r * f1);
            }
            else
            {
                f0 = new Vector3(Random.value, Random.value, 0);                
                ret = GenerateBone.from_to_zx(f0, r * f0);
            }
            if (i % 100 == 0)
                Debug.Log(r + "=" + ret);
        }
        */ 
    }
	
	// Update is called once per frame
	void Update () 
    {        
        trace_frame++;
        Vector3[] position = new Vector3[GenerateBone.TOTAL_PART];
        position[GenerateBone.HIP] = hip.position;
        position[GenerateBone.RIGHTUP_LEG] = rightup_leg.position;
        position[GenerateBone.LEFTUP_LEG] = leftup_leg.position;
        position[GenerateBone.RIGHT_LEG] = right_leg.position;
        position[GenerateBone.LEFT_LEG] = left_leg.position;
        position[GenerateBone.RIGHT_FOOT] = right_foot.position;
        position[GenerateBone.LEFT_FOOT] = left_foot.position;

        position[GenerateBone.SPINE] = spine.position;
        position[GenerateBone.RIGHT_ARM] = right_arm.position;
        position[GenerateBone.RIGHTFORE_ARM] = rightfore_arm.position;
        position[GenerateBone.RIGHT_HAND] = right_hand.position;
        position[GenerateBone.LEFT_ARM] = left_arm.position;
        position[GenerateBone.LEFTFORE_ARM] = leftfore_arm.position;
        position[GenerateBone.LEFT_HAND] = left_hand.position;
        position[GenerateBone.NECK] = neck.position;

        Vector3[] normal_pos;
        Quaternion[] normal_rot;
        GenerateBone.compute_normal(position, out normal_pos, out normal_rot);
        GenerateBone.apply_posture(normal_rot, mirror_body);
        
        if (anim.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Jump") && trace_behavior == TraceBehavior.TraceJump ||
            anim.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Run") && trace_behavior == TraceBehavior.TraceRun ||
            anim.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Roll") && trace_behavior == TraceBehavior.TraceRoll)
        {
            root_motion_trace.Add(position[GenerateBone.HIP]);
            hip_trace.Add(new Pose(normal_pos[GenerateBone.HIP], normal_rot[GenerateBone.HIP]));
            rightup_leg_trace.Add(new Pose(normal_pos[GenerateBone.RIGHTUP_LEG], normal_rot[GenerateBone.RIGHTUP_LEG]));
            leftup_leg_trace.Add(new Pose(normal_pos[GenerateBone.LEFTUP_LEG], normal_rot[GenerateBone.LEFTUP_LEG]));
            right_leg_trace.Add(new Pose(normal_pos[GenerateBone.RIGHT_LEG], normal_rot[GenerateBone.RIGHT_LEG]));
            left_leg_trace.Add(new Pose(normal_pos[GenerateBone.LEFT_LEG], normal_rot[GenerateBone.LEFT_LEG]));

            spine_trace.Add(new Pose(normal_pos[GenerateBone.SPINE], normal_rot[GenerateBone.SPINE]));
            right_arm_trace.Add(new Pose(normal_pos[GenerateBone.RIGHT_ARM], normal_rot[GenerateBone.RIGHT_ARM]));
            rightfore_arm_trace.Add(new Pose(normal_pos[GenerateBone.RIGHTFORE_ARM], normal_rot[GenerateBone.RIGHTFORE_ARM]));
            left_arm_trace.Add(new Pose(normal_pos[GenerateBone.LEFT_ARM], normal_rot[GenerateBone.LEFT_ARM]));
            leftfore_arm_trace.Add(new Pose(normal_pos[GenerateBone.LEFTFORE_ARM], normal_rot[GenerateBone.LEFTFORE_ARM]));
            neck_trace.Add(new Pose(normal_pos[GenerateBone.NECK], normal_rot[GenerateBone.NECK]));            
        }
                
	}

    void write_trace(StreamWriter sw, List<Pose> trace)
    {
        foreach (Pose p in trace) 
            sw.Write(p.position + "," + p.rotate.eulerAngles +"\n");
        sw.Write("relative\n");
        for (int i = 1; i < trace.Count; i++)
            sw.Write((trace[i].position - trace[0].position) + "\n");
    }

    void write_para(StreamWriter sw, List<Pose> trace)
    {
        sw.Write("{\n");
        int NUM = (trace_behavior == TraceBehavior.TraceRun) ? 36 : trace.Count;
        for(int i=0; i<NUM; i++) 
        {
            float w, x, y, z;
            w = trace[i].rotate.w;
            x = trace[i].rotate.x;
            y = trace[i].rotate.y;
            z = trace[i].rotate.z;
            
            x = (trace[i].rotate.eulerAngles.x >= 250) ? trace[i].rotate.eulerAngles.x - 360 : trace[i].rotate.eulerAngles.x;
            y = (trace[i].rotate.eulerAngles.y >= 250) ? trace[i].rotate.eulerAngles.y - 360 : trace[i].rotate.eulerAngles.y;
            z = (trace[i].rotate.eulerAngles.z >= 250) ? trace[i].rotate.eulerAngles.z - 360 : trace[i].rotate.eulerAngles.z;
            
            sw.Write("\t\t{" + x + "f, " + y + "f, " + z + "f}");
            if (i == NUM - 1)
                sw.Write("};\n");
            else
                sw.Write(",\n");
        }        
    }

    void write_para2(StreamWriter sw, List<Vector3> trace)
    {
        sw.Write("{\n");
        int NUM = trace.Count;
        for (int i=0; i<NUM; i++)
        {
            float x, y, z;
            x = (trace[i].x - trace[NUM-1].x) * MovePara.normalHeight * 0.55f;
            y = (trace[i].y - trace[NUM - 1].y) * MovePara.normalHeight * 0.55f;
            z = (trace[i].z - trace[NUM - 1].z) * MovePara.normalHeight * 0.55f;
            sw.Write("\t\t{" + x + "f, " + y + "f, " + z + "f}");
            if (i == NUM - 1)
                sw.Write("};\n");
            else
                sw.Write(",\n");
        }
    }
    void OnDestroy()
    {
        StreamWriter sw = new StreamWriter("trace.txt");
        sw.Write("hip_trace_run\n");
        write_trace(sw, hip_trace);
        sw.Write("\nleftup_leg_trace_run\n");
        write_trace(sw, leftup_leg_trace);
        sw.Write("\nleft_leg_trace_run\n");
        write_trace(sw, left_leg_trace);
        sw.Write("\nrightup_leg_trace_run\n");
        write_trace(sw, rightup_leg_trace);
        sw.Write("\nright_leg_trace_run\n");
        write_trace(sw, right_leg_trace);
        sw.Write("\nspine_run\n");
        write_trace(sw, spine_trace);
        sw.Write("\nleft_arm_trace_run\n");
        write_trace(sw, left_arm_trace);
        sw.Write("\nleftfore_arm_trace_run\n");
        write_trace(sw, leftfore_arm_trace);
        sw.Write("\nright_arm_trace_run\n");
        write_trace(sw, right_arm_trace);
        sw.Write("\nrightfore_arm_trace_run\n");
        write_trace(sw, rightfore_arm_trace);
        sw.Write("\nneck_trace_run\n");
        write_trace(sw, neck_trace);
        sw.Close();

        sw = new StreamWriter("MovePara.txt");
        string hip_name = "hip_rot_";
        string leftup_leg_name = "leftup_leg_rot_";
        string rightup_leg_name = "rightup_leg_rot_";
        string left_leg_name = "left_leg_rot_";
        string right_leg_name = "right_leg_rot_";
        string spine_name = "spine_rot_";
        string left_arm_name = "left_arm_rot_";
        string right_arm_name = "right_arm_rot_";
        string leftfore_arm_name = "leftfore_arm_rot_";
        string rightfore_arm_name = "rightfore_arm_rot_";
        string root_name = "base_pos_";
        switch (trace_behavior)
        {
            case TraceBehavior.TraceRun:
                hip_name = hip_name + "run";
                leftup_leg_name = leftup_leg_name + "run";
                rightup_leg_name = rightup_leg_name + "run";
                left_leg_name = left_leg_name + "run";
                right_leg_name = right_leg_name + "run";
                spine_name = spine_name + "run";
                left_arm_name = left_arm_name + "run";
                right_arm_name = right_arm_name + "run";
                leftfore_arm_name = leftfore_arm_name + "run";
                rightfore_arm_name = rightfore_arm_name + "run";
                root_name = root_name + "run";
                break;
            case TraceBehavior.TraceJump:
                hip_name = hip_name + "jump";
                leftup_leg_name = leftup_leg_name + "jump";
                rightup_leg_name = rightup_leg_name + "jump";
                left_leg_name = left_leg_name + "jump";
                right_leg_name = right_leg_name + "jump";
                spine_name = spine_name + "jump";
                left_arm_name = left_arm_name + "jump";
                right_arm_name = right_arm_name + "jump";
                leftfore_arm_name = leftfore_arm_name + "jump";
                rightfore_arm_name = rightfore_arm_name + "jump";
                root_name = root_name + "jump";
                break;
            case TraceBehavior.TraceRoll:
                hip_name = hip_name + "roll";
                leftup_leg_name = leftup_leg_name + "roll";
                rightup_leg_name = rightup_leg_name + "roll";
                left_leg_name = left_leg_name + "roll";
                right_leg_name = right_leg_name + "roll";
                spine_name = spine_name + "roll";
                left_arm_name = left_arm_name + "roll";
                right_arm_name = right_arm_name + "roll";
                leftfore_arm_name = leftfore_arm_name + "roll";
                rightfore_arm_name = rightfore_arm_name + "roll";
                root_name = root_name + "roll";
                break;
        }
        sw.Write("protected float [,] " + hip_name + ";\n");
        sw.Write("protected float [,] " + leftup_leg_name +";\n");
        sw.Write("protected float [,] " + rightup_leg_name +";\n");
        sw.Write("protected float [,] " + left_leg_name + ";\n");
        sw.Write("protected float [,] " + right_leg_name + ";\n");
        sw.Write("protected float [,] " + spine_name + ";\n");
        sw.Write("protected float [,] " + left_arm_name +";\n");
        sw.Write("protected float [,] " + right_arm_name +";\n");
        sw.Write("protected float [,] " + leftfore_arm_name + ";\n");
        sw.Write("protected float [,] " + rightfore_arm_name + ";\n");
        sw.Write("protected float [,] " + root_name + ";\n");

        sw.Write("\t" + hip_name + " = new float [,]");
        write_para(sw, hip_trace);
        sw.Write("\n\n\t" + leftup_leg_name + " = new float [,]");
        write_para(sw, leftup_leg_trace);
        sw.Write("\n\n\t" + rightup_leg_name + " = new float [,]");
        write_para(sw, rightup_leg_trace);
        sw.Write("\n\n\t" + left_leg_name + " = new float [,]");
        write_para(sw, left_leg_trace);
        sw.Write("\n\n\t" + right_leg_name + " = new float [,]");
        write_para(sw, right_leg_trace);
        sw.Write("\n\n\t" + spine_name + " = new float [,]");
        write_para(sw, spine_trace);
        sw.Write("\n\n\t" + left_arm_name + " = new float [,]");
        write_para(sw, left_arm_trace);
        sw.Write("\n\n\t"  + right_arm_name + " = new float [,]");
        write_para(sw, right_arm_trace);
        sw.Write("\n\n\t" + leftfore_arm_name + " = new float [,]");
        write_para(sw, leftfore_arm_trace);
        sw.Write("\n\n\t" + rightfore_arm_name + " = new float [,]");
        write_para(sw, rightfore_arm_trace);
        sw.Write("\n\n\t" + root_name + " = new float [,]");
        write_para2(sw, root_motion_trace); 
        sw.Close();
    }
}
