﻿using UnityEngine;
using System.Collections;

public class CreateSkin : MonoBehaviour {

	// Use this for initialization
    Quaternion bone0_ratation, bone1_ratation;
    public float Playspeed = 1.0f;
    void Start()
    {
        gameObject.AddComponent<Animation>();
        gameObject.AddComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(-1, 5, 0), new Vector3(1, 5, 0) };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.RecalculateNormals();
        renderer.material = new Material(Shader.Find(" Diffuse"));
        BoneWeight[] weights = new BoneWeight[4];
        weights[0].boneIndex0 = 0;
        weights[0].weight0 = 1;
        weights[1].boneIndex0 = 0;
        weights[1].weight0 = 1;
        weights[2].boneIndex0 = 1;
        weights[2].weight0 = 1;
        weights[3].boneIndex0 = 1;
        weights[3].weight0 = 1;
        mesh.boneWeights = weights;
        Transform[] bones = new Transform[2];
        Matrix4x4[] bindPoses = new Matrix4x4[2];
        bones[0] = new GameObject("Lower").transform;
        bones[0].parent = transform;
        bones[0].localRotation = Quaternion.identity;
        bone0_ratation = bones[0].localRotation;
        bones[0].localPosition = Vector3.zero;
        bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;
        bones[1] = new GameObject("Upper").transform;
        bones[1].parent = transform;
        bones[1].localRotation = new Quaternion(0, 0, 0.717f, 0.717f);
        bone1_ratation = bones[1].localRotation;
        bones[1].localPosition = new Vector3(0, 5, 0);
        bindPoses[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;
        mesh.bindposes = bindPoses;
        renderer.bones = bones;
        renderer.sharedMesh = mesh;
        AnimationCurve curve = new AnimationCurve();
        curve.keys = new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 3), new Keyframe(2, 0.0F) };
        AnimationClip clip = new AnimationClip();
        clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.y", curve);
        AnimationCurve curve1 = new AnimationCurve();
        curve1.keys = new Keyframe[] { new Keyframe(0, 5), new Keyframe(1, 3), new Keyframe(2, 5F) };
        clip.SetCurve("Upper", typeof(Transform), "m_LocalPosition.y", curve1);
        clip.wrapMode = WrapMode.Loop;
        animation.AddClip(clip, "test");
        animation.Play("test");
        
    }
    
    void Update()
    {
        /*
        if (Time.time > Mathf.PI)
        {
            SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
            Transform[] bones = renderer.bones;
            //bones[1].localPosition = new Vector3(0, 5+3* Mathf.Sin(Time.time) , 0);
            bones[1].localRotation = new Quaternion(0, Mathf.Sin(Time.time), 0, Mathf.Cos(Time.time));
            bones[1].localRotation = bones[1].localRotation * bone1_ratation;
            renderer.bones = bones;
        }*/
        animation["test"].speed = Playspeed;
    }
}
