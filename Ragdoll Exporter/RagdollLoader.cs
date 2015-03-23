/*
//  Copyright (c) 2014 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot]
public class Ragdoll
{
    [XmlElement]
    public RagdollJoint[] ragdollJoints;
}
public class RagdollJoint
{
    [XmlAttribute]
    public string boneName;
    [XmlElement]
    public CharacterJointSettings characterJointSettings;
    [XmlElement]
    public BoxColliderSettings boxColliderSettings;
    [XmlElement]
    public SphereColliderSettings sphereColliderSettings;
    [XmlElement]
    public CapsuleColliderSettings capsuleColliderSettings;
    [XmlElement]
    public RigidbodySettings rigidbodySettings;
}
public class CharacterJointSettings
{
    [XmlAttribute]
    public string connectedBody;

    [XmlAttribute]
    public string anchor;
    [XmlAttribute]
    public string axis;

    [XmlAttribute]
    public string connectedAnchor;
    [XmlAttribute]
    public string swingAxis;

    [XmlAttribute]
    public float lowTwistLimit_Limit;
    [XmlAttribute]
    public float lowTwistLimit_Bounciness;

    [XmlAttribute]
    public float highTwistLimit_Limit;
    [XmlAttribute]
    public float highTwistLimit_Bounciness;

    [XmlAttribute]
    public float swing1Limit_Limit;
    [XmlAttribute]
    public float swing1Limit_Bounciness;

    [XmlAttribute]
    public float swing2Limit_Limit;
    [XmlAttribute]
    public float swing2Limit_Bounciness;
}
public class BoxColliderSettings
{
    [XmlAttribute]
    public string center;
    [XmlAttribute]
    public string size;
}
public class SphereColliderSettings
{
    [XmlAttribute]
    public string center;
    [XmlAttribute]
    public float radius;
}
public class CapsuleColliderSettings
{
    [XmlAttribute]
    public string center;
    [XmlAttribute]
    public float height;
    [XmlAttribute]
    public float radius;
    [XmlAttribute]
    public int direction;
}
public class RigidbodySettings
{
    [XmlAttribute]
    public float mass;
}

public static class MathHelper
{
    public static Vector3 FromString(string value)
    {
        string[] temp = value.Replace(" ", "").Replace("(", "").Replace(")", "").Split(',');
        Vector3 vector = new Vector3();
        vector.x = float.Parse(temp[0]);
        vector.y = float.Parse(temp[1]);
        vector.z = float.Parse(temp[2]);
        return vector;
    }
}

public class RagdollLoader : MonoBehaviour {
    public Object ragdoll;
    public GameObject target;

	void Start () {
	}
	
    public static void Load(string ragdoll, GameObject target, bool editor)
    {
        Dictionary<string, RagdollJoint> ragdollJointsDictionary = new Dictionary<string, RagdollJoint>();
        RagdollJoint[] ragdollJoints = XMLSerializer.DeserializeObject<Ragdoll>(ragdoll).ragdollJoints;
        foreach (RagdollJoint rJ in ragdollJoints)
        {
            ragdollJointsDictionary[rJ.boneName] = rJ;
        }

        if (ragdollJointsDictionary.ContainsKey(target.name))
        {
            // Root bone

            if (target.GetComponent<CharacterJoint>() != null)
                DestroyImmediate(target.GetComponent<CharacterJoint>());
            if (target.GetComponent<Collider>() != null)
                DestroyImmediate(target.GetComponent<Collider>());
            if(target.GetComponent<Rigidbody>() != null)
                DestroyImmediate(target.GetComponent<Rigidbody>());
            Dictionary<string, GameObject> children = new Dictionary<string, GameObject>();
            foreach (Transform t in target.GetComponentsInChildren<Transform>())
            {
                children[t.gameObject.name] = t.gameObject;

                if (t.gameObject.GetComponent<CharacterJoint>() != null)
                    DestroyImmediate(t.gameObject.GetComponent<CharacterJoint>());
                if (t.gameObject.GetComponent<Collider>() != null)
                    DestroyImmediate(t.gameObject.GetComponent<Collider>());
                if (t.gameObject.GetComponent<Rigidbody>() != null)
                    DestroyImmediate(t.gameObject.GetComponent<Rigidbody>());
            }
            children[target.name] = target;

            RagdollJoint rJ = ragdollJointsDictionary[target.name];

            if (rJ.boxColliderSettings != null)
            {
                BoxCollider bC = target.AddComponent<BoxCollider>();
                bC.center = MathHelper.FromString(rJ.boxColliderSettings.center);
                bC.size = MathHelper.FromString(rJ.boxColliderSettings.size);
            }
            else if (rJ.sphereColliderSettings != null)
            {
                SphereCollider sC = target.AddComponent<SphereCollider>();
                sC.center = MathHelper.FromString(rJ.sphereColliderSettings.center);
                sC.radius = rJ.sphereColliderSettings.radius;
            }
            else if (rJ.capsuleColliderSettings != null)
            {
                CapsuleCollider cC = target.AddComponent<CapsuleCollider>();
                cC.center = MathHelper.FromString(rJ.capsuleColliderSettings.center);
                cC.radius = rJ.capsuleColliderSettings.radius;
                cC.height = rJ.capsuleColliderSettings.height;
                cC.direction = rJ.capsuleColliderSettings.direction;
            }
            else
                Debug.LogError("Ragdoll Loader: collider type is not supported (" + target.name + ")");

            Rigidbody rB = target.AddComponent<Rigidbody>();
            rB.mass = rJ.rigidbodySettings.mass;

            // Children

            if (editor)
            {
                Debug.Log("Ragdoll Loader: " + target.name + " joint processed");
            }

            int count = 1;

            foreach (GameObject gO in children.Values)
            {
                if (gO.name == target.name)
                    continue;

                if (ragdollJointsDictionary.ContainsKey(gO.name))
                {
                    rJ = ragdollJointsDictionary[gO.name];

                    if (rJ.boxColliderSettings != null)
                    {
                        BoxCollider bC = gO.AddComponent<BoxCollider>();
                        bC.center = MathHelper.FromString(rJ.boxColliderSettings.center);
                        bC.size = MathHelper.FromString(rJ.boxColliderSettings.size);
                    }
                    else if (rJ.sphereColliderSettings != null)
                    {
                        SphereCollider sC = gO.AddComponent<SphereCollider>();
                        sC.center = MathHelper.FromString(rJ.sphereColliderSettings.center);
                        sC.radius = rJ.sphereColliderSettings.radius;
                    }
                    else if (rJ.capsuleColliderSettings != null)
                    {
                        CapsuleCollider cC = gO.AddComponent<CapsuleCollider>();
                        cC.center = MathHelper.FromString(rJ.capsuleColliderSettings.center);
                        cC.radius = rJ.capsuleColliderSettings.radius;
                        cC.height = rJ.capsuleColliderSettings.height;
                        cC.direction = rJ.capsuleColliderSettings.direction;
                    }
                    else
                        Debug.LogError("Ragdoll Loader: collider type is not supported (" + gO.name + ")");

                    rB = gO.AddComponent<Rigidbody>();
                    rB.mass = rJ.rigidbodySettings.mass;

                    if (editor)
                    {
                        Debug.Log("Ragdoll Loader: " + gO.name + " joint processed");
                    }

                    count++;
                }
                else if (editor)
                {
                    //Debug.LogWarning("AssignRagdoll: bone name not found in ragdoll (" + gO.name + ")");
                }
            }

            foreach (GameObject gO in children.Values)
            {
                if (gO.name == target.name)
                    continue;

                if (ragdollJointsDictionary.ContainsKey(gO.name))
                {
                    rJ = ragdollJointsDictionary[gO.name];

                    CharacterJoint cJ = gO.AddComponent<CharacterJoint>();

                    if (children.ContainsKey(rJ.characterJointSettings.connectedBody))
                        cJ.connectedBody = children[rJ.characterJointSettings.connectedBody].GetComponent<Rigidbody>();
                    else
                        Debug.LogError("Ragdoll Loader: connected joint not found (" + rJ.characterJointSettings.connectedBody + " with " + gO.name + ")");

                    cJ.anchor = MathHelper.FromString(rJ.characterJointSettings.anchor);
                    cJ.axis = MathHelper.FromString(rJ.characterJointSettings.axis);
                    cJ.swingAxis = MathHelper.FromString(rJ.characterJointSettings.swingAxis);
                    cJ.connectedAnchor = MathHelper.FromString(rJ.characterJointSettings.connectedAnchor);

                    float spring = 225F;
                    float damper = 10F;
                    SoftJointLimitSpring sJLS = new SoftJointLimitSpring();
                    sJLS.damper = damper;
                    sJLS.spring = spring;
                    cJ.twistLimitSpring = sJLS;
                    sJLS = new SoftJointLimitSpring();
                    sJLS.damper = damper;
                    sJLS.spring = spring;
                    cJ.swingLimitSpring = sJLS;

                    float bounciness = .00F; // Joint bounciness when it hits the limit
                    float contactDistance = 0; // 0 = Default

                    SoftJointLimit sJL = new SoftJointLimit();
                    sJL.bounciness = bounciness;//rJ.characterJointSettings.lowTwistLimit_Bounciness;
                    sJL.limit = rJ.characterJointSettings.lowTwistLimit_Limit;
                    sJL.contactDistance = contactDistance;
                    cJ.lowTwistLimit = sJL;

                    sJL = new SoftJointLimit();
                    sJL.bounciness = bounciness;//rJ.characterJointSettings.highTwistLimit_Bounciness;
                    sJL.limit = rJ.characterJointSettings.highTwistLimit_Limit;
                    sJL.contactDistance = contactDistance;
                    cJ.highTwistLimit = sJL;

                    sJL = new SoftJointLimit();
                    sJL.bounciness = bounciness;//rJ.characterJointSettings.swing1Limit_Bounciness;
                    sJL.limit = rJ.characterJointSettings.swing1Limit_Limit;
                    sJL.contactDistance = contactDistance;
                    cJ.swing1Limit = sJL;

                    sJL = new SoftJointLimit();
                    sJL.bounciness = bounciness;//rJ.characterJointSettings.swing2Limit_Bounciness;
                    sJL.limit = rJ.characterJointSettings.swing2Limit_Limit;
                    sJL.contactDistance = contactDistance;
                    cJ.swing2Limit = sJL;
                }
            }

            if (editor)
            {
                Debug.Log("Ragdoll Loader: " + count.ToString() + " joints processed");
                Debug.Log("Success!");
            }
        }
        else
            Debug.LogError("Ragdoll Loader: target needs to be the root bone!");
    }
}
