using Godot;
using System;
using System.Collections.Generic;
using Numerics = System.Numerics;

using FreedomOfFormFoundation.AnatomyEngine;
using Anatomy = FreedomOfFormFoundation.AnatomyEngine.Anatomy;
using FreedomOfFormFoundation.AnatomyEngine.Anatomy.Bones;
using FreedomOfFormFoundation.AnatomyEngine.Geometry;
using FreedomOfFormFoundation.AnatomyEngine.Calculus;
using FreedomOfFormFoundation.AnatomyEngine.Renderable;

namespace FreedomOfFormFoundation.AnatomyRenderer
{
	public class ExampleBone : MeshInstance
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			// Example method that creates a character and adds a single joint and bone:
			Anatomy.Skeleton skeleton = new Anatomy.Skeleton();
			
			CreateExampleJoint(skeleton);
			CreateExampleBones(skeleton);
			
			// Test:
			List<float> intersections = QuarticFunction.Solve(5.0f, 8.0f, 2.0f, -2.0f, -7.0f);
			foreach (float i in intersections)
			{
				Console.WriteLine("quarticfunction solve: " + i);
			}
			
			// Should be: -1.3162 and 1.2934 and -0.13149 +- 0.90659 i.
			
			Console.WriteLine("END QUARTIC TEST");
			
			// End test.
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			
		}
		
		public void CreateExampleBones(Anatomy.Skeleton skeleton)
		{
			// Generate a simple cubic spline that will act as the radius of a long bone:
			SortedList<float, float> radiusPoints = new SortedList<float, float>();
			radiusPoints.Add(-3.5f, 0.7f*0.92f);
			radiusPoints.Add(0.02f, 0.7f*0.92f);
			radiusPoints.Add(0.15f, 0.7f*0.8f);
			radiusPoints.Add(0.5f, 0.7f*0.7f);
			radiusPoints.Add(0.8f, 0.7f*0.76f);
			radiusPoints.Add(0.98f, 0.7f*0.8f);
			radiusPoints.Add(4.5f, 0.7f*0.8f);
			
			CubicSpline1D boneRadius = new CubicSpline1D(radiusPoints);

			// Define the center curve of the long bone:
			SortedList<float, Numerics.Vector3> centerPoints = new SortedList<float, Numerics.Vector3>();
			centerPoints.Add(0.0f, new Numerics.Vector3(0.0f, 0.0f, 2.7f));
			centerPoints.Add(0.25f, new Numerics.Vector3(-0.3f, -0.5f, 1.0f));
			centerPoints.Add(0.5f, new Numerics.Vector3(0.3f, 1.0f, 0.0f));
			centerPoints.Add(0.75f, new Numerics.Vector3(0.8f, 1.0f, -1.0f));
			centerPoints.Add(1.0f, new Numerics.Vector3(0.6f, -0.5f, -0.9f));
			
			SpatialCubicSpline boneCenter = new SpatialCubicSpline(centerPoints);
			
			// Add first bone:
			Line centerLine = new Line(new Numerics.Vector3(0.0f, 0.3f, 0.5f),
									   new Numerics.Vector3(0.001f, 10f, 0.51f));
			
			var bone1 = new Anatomy.Bones.LongBone(centerLine, boneRadius);
			bone1.InteractingJoints.Add((skeleton.joints[0], RayCastDirection.Outwards));
			skeleton.bones.Add(bone1);
			
			// Add second bone:
			Line centerLine2 = new Line(new Numerics.Vector3(0.0f, 0.2f, 0.5f),
									   new Numerics.Vector3(0.001f, -10.0f, 0.51f));
			
			var bone2 = new Anatomy.Bones.LongBone(centerLine2, 0.3f);
			bone2.InteractingJoints.Add((skeleton.joints[0], RayCastDirection.Inwards));
			skeleton.bones.Add(bone2);
			
			// Generate the geometry vertices of the first bone with resolution U=128 and resolution V=128:
			foreach ( var bone in skeleton.bones )
			{
				UVMesh mesh = bone.GetGeometry().GenerateMesh(128, 128);
				
				// Finally upload the mesh to Godot:
				MeshInstance newMesh = new MeshInstance();
				newMesh.Mesh = new GodotMeshConverter(mesh);
				
				// Give each mesh a random color:
				var boneMaterial = (SpatialMaterial)GD.Load("res://BoneMaterial.tres").Duplicate();
				boneMaterial.AlbedoColor = new Color(GD.Randf(), GD.Randf(), GD.Randf(), GD.Randf());
				newMesh.SetSurfaceMaterial(0, boneMaterial);
				
				AddChild(newMesh);
			}
		}
		
		public void CreateExampleJoint(Anatomy.Skeleton skeleton)
		{
			// Generate a simple cubic spline that will act as the radius of a long bone:
			SortedList<float, float> splinePoints = new SortedList<float, float>();
			float radiusModifier = 0.6f;
			splinePoints.Add(-0.1f, radiusModifier*1.1f);
			splinePoints.Add(0.0f, radiusModifier*1.1f);
			splinePoints.Add(0.15f, radiusModifier*0.95f);
			splinePoints.Add(0.3f, radiusModifier*0.9f);
			splinePoints.Add(0.5f, radiusModifier*1.2f);
			splinePoints.Add(0.7f, radiusModifier*0.9f);
			splinePoints.Add(0.8f, radiusModifier*0.95f);
			splinePoints.Add(1.0f, radiusModifier*1.1f);
			
			QuadraticSpline1D jointSpline = new QuadraticSpline1D(splinePoints);

			// Define the center curve of the long bone:
			Line centerLine = new Line(new Numerics.Vector3(0.0f, 0.0f, 0.0f),
									   new Numerics.Vector3(0.01f, 0.0f, 1.0f));
			
			// Add a long bone to the character:
			skeleton.joints.Add(new Anatomy.Joints.HingeJoint(centerLine, jointSpline, -0.5f*(float)Math.PI, 0.5f*(float)Math.PI));
			
			// Generate the geometry vertices of the first bone with resolution U=32 and resolution V=32:
			UVMesh mesh = skeleton.joints[0].GetArticularSurface().GenerateMesh(64, 64);
			
			// Finally upload the mesh to Godot:
			MeshInstance newMesh = new MeshInstance();
			newMesh.Mesh = new GodotMeshConverter(mesh);
			newMesh.SetSurfaceMaterial(0, (Material)GD.Load("res://JointMaterial.tres"));
			
			//AddChild(newMesh);
		}
	}
}
