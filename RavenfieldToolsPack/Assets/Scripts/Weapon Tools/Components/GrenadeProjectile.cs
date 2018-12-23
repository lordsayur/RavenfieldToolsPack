﻿using UnityEngine;
using System.Collections;

public class GrenadeProjectile : Projectile {

	const float CLEANUP_TIME = 10f;
	const float ROTATION_SPEED_MAGNITUDE = 400f;

	public ExplodingProjectile.ExplosionConfiguration explosionConfiguration;

	public Renderer[] renderers;

	public float radius = 0.1f;
	public float bounciness = 0.5f;
	public float bounceDrag = 0.2f;

	Vector3 rotationAxis;
	float angularSpeed;

	protected override void Start ()
	{
		this.velocity = this.transform.forward*this.configuration.speed;
		this.rotationAxis = Random.insideUnitSphere.normalized;
		this.angularSpeed = ROTATION_SPEED_MAGNITUDE;
		Invoke("Explode", this.configuration.lifetime);
	}

	protected override void Update ()
	{
		this.velocity += Physics.gravity*Time.deltaTime;
		Vector3 deltaPosition = velocity*Time.deltaTime;
		Ray ray = new Ray(this.transform.position, deltaPosition);
		RaycastHit hitInfo;
		if(Physics.SphereCast(ray, this.radius, out hitInfo, deltaPosition.magnitude*2f, 1)) {
			this.transform.position = hitInfo.point+hitInfo.normal*(this.radius+0.01f);
			Vector3 normalVelocityComponent = Vector3.Project(this.velocity, hitInfo.normal);
			velocity -= normalVelocityComponent*(bounciness+1f);
			Vector3 lostVelocity = velocity*bounceDrag;
			velocity -= lostVelocity;
			this.rotationAxis = this.transform.worldToLocalMatrix.MultiplyVector((Vector3.Cross(lostVelocity, Vector3.up)+this.rotationAxis).normalized);
			this.angularSpeed = -lostVelocity.magnitude*ROTATION_SPEED_MAGNITUDE;
		}
		else {
			this.transform.position += deltaPosition;
		}

		this.transform.Rotate(this.rotationAxis, angularSpeed*Time.deltaTime);
	}

	protected virtual void Explode () {

		this.transform.rotation = Quaternion.LookRotation(Vector3.up);

		this.enabled = false;
		
		foreach(Renderer r in this.renderers) {
			r.enabled = false;
		}
		this.GetComponent<ParticleSystem>().Play(true);

		AudioSource audioSource = this.GetComponent<AudioSource>();
		audioSource.pitch = Random.Range(0.9f, 1.1f);
		audioSource.Play();

		Invoke("Cleanup", CLEANUP_TIME);
	}
	
	void Cleanup() {
		Destroy(this.gameObject);
	}
}
