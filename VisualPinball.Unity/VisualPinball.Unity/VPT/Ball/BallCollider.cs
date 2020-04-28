﻿using Unity.Mathematics;
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Common;
using VisualPinball.Unity.Physics.Collision;

namespace VisualPinball.Unity.VPT.Ball
{
	public static class BallCollider
	{
		private const float HardScatter = 0.0f;

		public static void Collide3DWall(ref BallData ball, in PhysicsMaterialData material, in CollisionEventData coll, in float3 hitNormal, ref Random random)
		{
			// speed normal to wall
			var dot = math.dot(ball.Velocity, hitNormal);

			if (dot >= -PhysicsConstants.LowNormVel) {
				// nearly receding ... make sure of conditions
				if (dot > PhysicsConstants.LowNormVel) {
					// otherwise if clearly approaching .. process the collision
					return; // is this velocity clearly receding (i.E must > a minimum)
				}

				if (coll.HitDistance < -PhysicsConstants.Embedded) {
					dot = -PhysicsConstants.EmbedShot; // has ball become embedded???, give it a kick

				} else {
					return;
				}
			}

			// correct displacements, mostly from low velocity, alternative to acceleration processing
			var hDist = -PhysicsConstants.DispGain * coll.HitDistance; // limit delta noise crossing ramps,
			if (hDist > 1.0e-4) {
				// when hit detection checked it what was the displacement
				if (hDist > PhysicsConstants.DispLimit) {
					hDist = PhysicsConstants.DispLimit; // crossing ramps, delta noise
				}

				// push along norm, back to free area
				ball.Position += hitNormal * hDist;
				// use the norm, but this is not correct, reverse time is correct
			}

			// magnitude of the impulse which is just sufficient to keep the ball from
			// penetrating the wall (needed for friction computations)
			var reactionImpulse = ball.Mass * math.abs(dot);

			var elasticity = Math.ElasticityWithFalloff(material.Elasticity, material.ElasticityFalloff, dot);
			dot *= -(1.0f + elasticity);
			ball.Velocity += hitNormal * dot;                                  // apply collision impulse (along normal, so no torque)

			// compute friction impulse
			var surfP = -ball.Radius * hitNormal;                        // surface contact point relative to center of mass
			var surfVel = BallData.SurfaceVelocity(in ball, in surfP);                              // velocity at impact point
			var tangent = surfVel - hitNormal * math.dot(surfVel, hitNormal);  // calc the tangential velocity

			var tangentSpSq = math.lengthsq(tangent);
			if (tangentSpSq > 1e-6) {
				tangent /= math.sqrt(tangentSpSq);                        // normalize to get tangent direction
				var vt = math.dot(surfVel, tangent);                      // get speed in tangential direction

				// compute friction impulse
				var cross = math.cross(surfP, tangent); // todo check this does the same as Vertex3D.CrossProduct
				var kt = 1f / ball.Mass + math.dot(tangent, math.cross(cross / ball.Inertia, surfP));

				// friction impulse can't be greater than coefficient of friction times collision impulse (Coulomb friction cone)
				var maxFric = material.Friction * reactionImpulse;
				var jt = math.clamp(-vt / kt, -maxFric, maxFric);

				if (!float.IsNaN(jt) && !float.IsInfinity(jt)) {
					ball.ApplySurfaceImpulse(jt * cross, jt * tangent);
				}
			}

			var scatterAngle = material.Scatter;
			if (scatterAngle < 0.0) {
				scatterAngle = HardScatter;
			} // if < 0 use global value

			// todo don't hardcode
			scatterAngle *= 0.2f; //_tableData.GlobalDifficulty; // apply difficulty weighting

			if (dot > 1.0 && scatterAngle > 1.0e-5) {
				// no scatter at low velocity
				var scatter = random.NextFloat(-1f, 1f);                            // -1.0f..1.0f
				scatter *= (1.0f - scatter * scatter) * 2.59808f * scatterAngle;         // shape quadratic distribution and scale
				var radSin = math.sin(scatter);                               // Green's transform matrix... rotate angle delta
				var radCos = math.cos(scatter);                               // rotational transform from current position to position at time t
				var vxt = ball.Velocity.x;
				var vyt = ball.Velocity.y;
				ball.Velocity.x = vxt * radCos - vyt * radSin;                           // rotate to random scatter angle
				ball.Velocity.y = vyt * radCos + vxt * radSin;
			}
		}

		public static void HandleStaticContact(ref BallData ball, in CollisionEventData coll, float friction, float dTime, in float3 gravity)
		{
			// this should be zero, but only up to +/- PhysicsConstants.ContactVel
			var normVel = math.dot(ball.Velocity, coll.HitNormal);

			// If some collision has changed the ball's velocity, we may not have to do anything.
			if (normVel <= PhysicsConstants.ContactVel) {

				// external forces (only gravity for now)
				var fe = gravity * ball.Mass;
				var dot = math.dot(fe, coll.HitNormal);

				// normal force is always nonnegative
				var normalForce = math.max(0.0f, -(dot * dTime + coll.HitOrgNormalVelocity));

				// Add just enough to kill original normal velocity and counteract the external forces.
				ball.Velocity += coll.HitNormal * normalForce;

				ApplyFriction(ref ball, coll.HitNormal, dTime, friction, gravity);
			}
		}

		public static void ApplyFriction(ref BallData ball, in float3 hitNormal, float dTime, float frictionCoeff, in float3 gravity)
		{
			// surface contact point relative to center of mass
			var surfP = -ball.Radius * hitNormal;
			var surfVel = BallData.SurfaceVelocity(in ball, in surfP);

			// calc the tangential slip velocity
			var slip = surfVel - hitNormal * math.dot(surfVel, hitNormal);

			var maxFriction = frictionCoeff * ball.Mass * -math.dot(gravity, hitNormal);

			var slipSpeed = math.length(slip);
			float3 slipDir;
			float numer;

			var normVel = math.dot(ball.Velocity, hitNormal);
			if (normVel <= 0.025 || slipSpeed < PhysicsConstants.Precision) {
				// check for <=0.025 originated from ball<->rubber collisions pushing the ball upwards, but this is still not enough, some could even use <=0.2
				// slip speed zero - static friction case

				var surfAcc = BallData.SurfaceAcceleration(in ball, in surfP, in gravity);
				// calc the tangential slip acceleration
				var slipAcc = surfAcc - hitNormal * math.dot(surfAcc, hitNormal);

				// neither slip velocity nor slip acceleration? nothing to do here
				if (math.lengthsq(slipAcc) < 1e-6) {
					return;
				}

				slipDir = math.normalize(slipAcc);
				numer = -math.dot(slipDir, surfAcc);

			} else {
				// nonzero slip speed - dynamic friction case
				slipDir = slip / slipSpeed;
				numer = -math.dot(slipDir, surfVel);
			}

			var cp = math.cross(surfP, slipDir);
			var denom = 1.0f / ball.Mass + math.dot(slipDir, math.cross(cp / ball.Inertia, surfP));
			var friction = math.clamp(numer / denom, -maxFriction, maxFriction);

			if (!float.IsNaN(friction) && !float.IsInfinity(friction)) {
				ball.ApplySurfaceImpulse(dTime * friction * cp, dTime * friction * slipDir);
			}
		}

		public static float HitTest(ref CollisionEventData collEvent, ref BallData hittingBall, in BallData ball, float dTime)
		{
			var d = ball.Position - hittingBall.Position;                    // delta position
			var dv = ball.Velocity - hittingBall.Velocity;                            // delta velocity

			var bcddSq = math.lengthsq(d);                                         // square of ball center"s delta distance
			var bcdd = math.sqrt(bcddSq);                                     // length of delta

			if (bcdd < 1.0e-8) {
				// two balls center-over-center embedded
				d.z = -1.0f;                                                   // patch up
				hittingBall.Position.z -= d.z;                                       // lift up

				bcdd = 1.0f;                                                   // patch up
				bcddSq = 1.0f;                                                 // patch up
				dv.z = 0.1f;                                                   // small speed difference
				hittingBall.Velocity.z -= dv.z;
			}

			var b = math.dot(dv, d);                                                 // inner product
			var bnv = b / bcdd;                                                // normal speed of balls toward each other

			if (bnv > PhysicsConstants.LowNormVel) {
				// dot of delta velocity and delta displacement, positive if receding no collision
				return -1.0f;
			}

			var totalRadius = hittingBall.Radius + ball.Radius;
			var bnd = bcdd - totalRadius;                                      // distance between ball surfaces

			float hitTime;
			var isContact = false;
			if (bnd <= PhysicsConstants.PhysTouch) {
				// in contact?
				if (bnd < hittingBall.Radius * -2.0f) {
					return -1.0f;                                              // embedded too deep?
				}

				if (math.abs(bnv) > PhysicsConstants.ContactVel               // >fast velocity, return zero time
				    || bnd <= -PhysicsConstants.PhysTouch) {
					// zero time for rigid fast bodies
					hitTime = 0; // slow moving but embedded

				} else {
					hitTime = bnd / -bnv;
				}

				if (math.abs(bnv) <= PhysicsConstants.ContactVel) {
					isContact = true;
				}

			} else {
				var a = math.lengthsq(dv);                                         // square of differential velocity
				if (a < 1.0e-8) {
					// ball moving really slow, then wait for contact
					return -1.0f;
				}

				var solved = Math.SolveQuadraticEq(a, 2.0f * b, bcddSq - totalRadius * totalRadius,
					out var time1, out var time2);
				if (!solved) {
					return -1.0f;
				}

				hitTime = time1 * time2 < 0
					? math.max(time1, time2)
					: math.min(time1, time2);                                 // find smallest nonnegative solution
			}

			if (float.IsNaN(hitTime) || float.IsInfinity(hitTime) || hitTime < 0 || hitTime > dTime) {
				// .. was some time previous || beyond the next physics tick
				return -1.0f;
			}

			var hitPos = hittingBall.Position + hitTime * dv; // new ball position

			// calc unit normal of collision
			var hitNormal = hitPos - ball.Position;
			if (math.abs(hitNormal.x) <= Constants.FloatMin && math.abs(hitNormal.y) <= Constants.FloatMin &&
			    math.abs(hitNormal.z) <= Constants.FloatMin) {
				return -1.0f;
			}

			collEvent.HitNormal = math.normalize(hitNormal);
			collEvent.HitDistance = bnd;                                            // actual contact distance
			collEvent.IsContact = isContact;
			if (isContact) {
				collEvent.HitOrgNormalVelocity = bnv;
			}

			return hitTime;
		}

		public static void Collide(ref BallData ball, ref BallData collidingBall,
			in CollisionEventData ballCollEvent, in CollisionEventData collidingCollEvent,
			bool swapBallCollisionHandling)
		{
			// make sure we process each ball/ball collision only once
			// (but if we are frozen, there won't be a second collision event, so deal with it now!)
			if ((swapBallCollisionHandling && collidingBall.Id >= ball.Id ||
			     !swapBallCollisionHandling && collidingBall.Id <= ball.Id) && !ball.IsFrozen) {
				return;
			}

			// target ball to object ball delta velocity
			var vRel = collidingBall.Velocity - ball.Velocity;
			var vNormal = collidingCollEvent.HitNormal;
			var dot = math.dot(vRel, vNormal);

			// correct displacements, mostly from low velocity, alternative to true acceleration processing
			if (dot >= -PhysicsConstants.LowNormVel) {

				// nearly receding ... make sure of conditions
				if (dot > PhysicsConstants.LowNormVel) {

					// otherwise if clearly approaching .. process the collision
					return; // is this velocity clearly receding (i.E must > a minimum)
				}

				//#ifdef PhysicsConstants.Embedded
				if (collidingCollEvent.HitDistance < -PhysicsConstants.Embedded) {
					dot = -PhysicsConstants.EmbedShot; // has ball become embedded???, give it a kick

				} else {
					return;
				}

				//#endif
			}

			// fixme script
			// send ball/ball collision event to script function
			// if (dot < -0.25f) {   // only collisions with at least some small true impact velocity (no contacts)
			//      g_pplayer->m_ptable->InvokeBallBallCollisionCallback(this, pball, -dot);
			// }

			//#ifdef PhysicsConstants.DispGain
			var eDist = -PhysicsConstants.DispGain * collidingCollEvent.HitDistance;
			var normalDist = eDist * vNormal;
			if (eDist > 1.0e-4) {
				if (eDist > PhysicsConstants.DispLimit) {
					eDist = PhysicsConstants.DispLimit; // crossing ramps, delta noise
				}

				if (!ball.IsFrozen) {
					// if the hit ball is not frozen
					eDist *= 0.5f;
				}

				collidingBall.Position += normalDist; // push along norm, back to free area
				// use the norm, but is not correct, but cheaply handled
			}

			eDist = -PhysicsConstants.DispGain * ballCollEvent.HitDistance; // noisy value .... needs investigation
			if (!ball.IsFrozen && eDist > 1.0e-4) {
				if (eDist > PhysicsConstants.DispLimit) {
					eDist = PhysicsConstants.DispLimit; // crossing ramps, delta noise
				}

				eDist *= 0.5f;
				ball.Position -= normalDist; // pull along norm, back to free area
			}
			//#endif

			var myInvMass = ball.IsFrozen ? 0.0f : ball.InvMass; // frozen ball has infinite mass
			var impulse = -(1.0f + 0.8f) * dot / (myInvMass + collidingBall.InvMass); // resitution = 0.8

			if (!ball.IsFrozen) {
				ball.Velocity -= impulse * myInvMass * vNormal;
			}

			collidingBall.Velocity += impulse * collidingBall.InvMass * vNormal;
		}
	}
}
