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

using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

public class RagdollExporter : ScriptableWizard
{
    private string xml = null;
    private string path = "Ragdoll";
    public GameObject rootBone = null;
    private bool export;

    [MenuItem("Tools/Ragdoll/Exporter")]
    static void CreateWindowExport()
    {
        ((RagdollExporter)ScriptableWizard.DisplayWizard("Select root bone of ragdoll:", typeof(RagdollExporter), "Next")).export = true;
    }

    [MenuItem("Tools/Ragdoll/Loader")]
    static void CreateWindowLoad()
    {
        ((RagdollExporter)ScriptableWizard.DisplayWizard("Select root bone of ragdoll:", typeof(RagdollExporter), "Next")).export = false;
    }

    void OnWizardCreate()
    {
        if (!export)
        {
            string path = EditorUtility.OpenFilePanel("Load Ragdoll", "Assets", "xml");
            if (path.Length != 0)
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                xml = XMLSerializer.ByteArrayToString(bytes);

                RagdollLoader.Load(xml, rootBone, true);
            }
        }
        else
        {

            if ((Selection.gameObjects == null) || (Selection.gameObjects.Length != 1))
            {
                EditorUtility.DisplayDialog("Wrong selection", "Please select root bone to export the ragdoll", "OK");
                return;
            }
            path = EditorUtility.SaveFilePanel("Save Ragdoll", "Assets", "Ragdoll", "xml");
            GameObject gameObject = rootBone;
            List<RagdollJoint> ragdollJoints = new List<RagdollJoint>();

            CharacterJoint[] jointsFromChildren = gameObject.GetComponents<CharacterJoint>();
            if (jointsFromChildren.Length == 0)
            {
                Collider rootCollider = gameObject.GetComponent<Collider>();
                Rigidbody rootRigidBody = gameObject.GetComponent<Rigidbody>();

                if (rootCollider == null || rootRigidBody == null)
                {
                    Debug.LogError("Ragdoll Exporter: root bone needs a collider and a rigidbody!");
                    return;
                }

                BoxColliderSettings boxColliderSettings = null;
                SphereColliderSettings sphereColliderSettings = null;
                CapsuleColliderSettings capsuleColliderSettings = null;
                if (rootCollider is BoxCollider)
                {
                    BoxCollider cast = (BoxCollider)rootCollider;
                    boxColliderSettings = new BoxColliderSettings();
                    boxColliderSettings.center = cast.center.ToString("G4");
                    boxColliderSettings.size = cast.size.ToString("G4");
                }
                else if (rootCollider is SphereCollider)
                {
                    SphereCollider cast = (SphereCollider)rootCollider;
                    sphereColliderSettings = new SphereColliderSettings();
                    sphereColliderSettings.center = cast.center.ToString("G4");
                    sphereColliderSettings.radius = cast.radius;
                }
                else if (rootCollider is CapsuleCollider)
                {
                    CapsuleCollider cast = (CapsuleCollider)rootCollider;
                    capsuleColliderSettings = new CapsuleColliderSettings();
                    capsuleColliderSettings.center = cast.center.ToString("G4");
                    capsuleColliderSettings.radius = cast.radius;
                    capsuleColliderSettings.height = cast.height;
                    capsuleColliderSettings.direction = cast.direction;
                }

                RigidbodySettings rigidbodySettings = new RigidbodySettings();
                rigidbodySettings.mass = rootRigidBody.mass;

                RagdollJoint ragdollJoint = new RagdollJoint();
                ragdollJoint.boneName = gameObject.name;
                ragdollJoint.characterJointSettings = null;
                ragdollJoint.boxColliderSettings = boxColliderSettings;
                ragdollJoint.sphereColliderSettings = sphereColliderSettings;
                ragdollJoint.capsuleColliderSettings = capsuleColliderSettings;
                ragdollJoint.rigidbodySettings = rigidbodySettings;
                ragdollJoints.Add(ragdollJoint);
            }
            else
            {
                EditorUtility.DisplayDialog("Wrong selection", "Please select root bone to export the ragdoll", "OK");
                return;
            }

            // Iterate over all character joints because it is easy to get rigidbody and collider from a joint but not the other way around
            jointsFromChildren = gameObject.GetComponentsInChildren<CharacterJoint>();
            Debug.Log("Ragdoll Exporter: " + gameObject.name + " joint processed");
            foreach (CharacterJoint joint in jointsFromChildren)
            {
                Rigidbody rigidbody = joint.gameObject.GetComponent<Rigidbody>();
                Collider collider = joint.gameObject.GetComponent<Collider>();
                if (rigidbody == null || collider == null)
                {
                    Debug.LogWarning("Ragdoll Exporter: bone with CharacterJoint is missing a collider or rigidbody (" + joint.name + ")");
                    continue;
                }

                CharacterJointSettings characterJointSettings = new CharacterJointSettings();
                characterJointSettings.connectedBody = joint.connectedBody.name;

                characterJointSettings.anchor = joint.anchor.ToString("G4");
                characterJointSettings.axis = joint.axis.ToString("G4");
                characterJointSettings.connectedAnchor = joint.connectedAnchor.ToString("G4");
                characterJointSettings.swingAxis = joint.swingAxis.ToString("G4");

                characterJointSettings.lowTwistLimit_Bounciness = joint.lowTwistLimit.bounciness;
                characterJointSettings.lowTwistLimit_Limit = joint.lowTwistLimit.limit;

                characterJointSettings.highTwistLimit_Bounciness = joint.highTwistLimit.bounciness;
                characterJointSettings.highTwistLimit_Limit = joint.highTwistLimit.limit;

                characterJointSettings.swing1Limit_Bounciness = joint.swing1Limit.bounciness;
                characterJointSettings.swing1Limit_Limit = joint.swing1Limit.limit;

                characterJointSettings.swing2Limit_Bounciness = joint.swing2Limit.bounciness;
                characterJointSettings.swing2Limit_Limit = joint.swing2Limit.limit;

                BoxColliderSettings boxColliderSettings = null;
                SphereColliderSettings sphereColliderSettings = null;
                CapsuleColliderSettings capsuleColliderSettings = null;
                if (collider is BoxCollider)
                {
                    BoxCollider cast = (BoxCollider)collider;
                    boxColliderSettings = new BoxColliderSettings();
                    boxColliderSettings.center = cast.center.ToString("G4");
                    boxColliderSettings.size = cast.size.ToString("G4");
                }
                else if (collider is SphereCollider)
                {
                    SphereCollider cast = (SphereCollider)collider;
                    sphereColliderSettings = new SphereColliderSettings();
                    sphereColliderSettings.center = cast.center.ToString("G4");
                    sphereColliderSettings.radius = cast.radius;
                }
                else if (collider is CapsuleCollider)
                {
                    CapsuleCollider cast = (CapsuleCollider)collider;
                    capsuleColliderSettings = new CapsuleColliderSettings();
                    capsuleColliderSettings.center = cast.center.ToString("G4");
                    capsuleColliderSettings.radius = cast.radius;
                    capsuleColliderSettings.height = cast.height;
                    capsuleColliderSettings.direction = cast.direction;
                }

                RigidbodySettings rigidbodySettings = new RigidbodySettings();
                rigidbodySettings.mass = rigidbody.mass;

                RagdollJoint ragdollJoint = new RagdollJoint();
                ragdollJoint.boneName = joint.name;
                ragdollJoint.characterJointSettings = characterJointSettings;
                ragdollJoint.boxColliderSettings = boxColliderSettings;
                ragdollJoint.sphereColliderSettings = sphereColliderSettings;
                ragdollJoint.capsuleColliderSettings = capsuleColliderSettings;
                ragdollJoint.rigidbodySettings = rigidbodySettings;
                ragdollJoints.Add(ragdollJoint);

                Debug.Log("Ragdoll Exporter: " + joint.name + " joint processed");
            }

            if (path == null || path == "")
            {
                Debug.Log("Ragdoll Exporter: operation cancelled");
                return;
            }
            Ragdoll rD = new Ragdoll();
            rD.ragdollJoints = ragdollJoints.ToArray();
            string xml = XMLSerializer.SerializeObject(rD);
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                try
                {
                    writer.Write(xml);
                }
                catch (System.Exception ex)
                {
                    string msg = " threw:\n" + ex.ToString();
                    Debug.LogError(msg);
                    EditorUtility.DisplayDialog("Error on export", msg, "OK");
                }
            }

            Debug.Log("Ragdoll Exporter: " + (jointsFromChildren.Length + 1).ToString() + " joints processed");
            Debug.Log("Success!");
        }
    }
}