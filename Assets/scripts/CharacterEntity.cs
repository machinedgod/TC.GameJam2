using UnityEngine;
using System.Collections;

public enum CharacterMovementState
{
	Standing,
	Walking,
	Running,
	Jumping,
	Leveling,
	Falling
}

public class CharacterEntity : StageEntity 
{	
	private Rigidbody rigidBody;
	
	private Vector3? startPos;
		
	private CharacterMovementState movementState;
	
	private int levelingTimer;
	private int moveTimeoutTimer;
	
	private Vector3 lastGroundedPosition;
	private Vector2 moveDirection;
			
	// ---------------------------------------------
	// Public
	// ---------------------------------------------
	public float Speed = 1.0f;
	public float AirSpeed = 1.0f;
	public float RunSpeed = 2.0f;
	
	public float JumpStrength = 3.0f;
	
	public float DeathDepth = -3;
	
	public bool CanMoveInAir = false;
	
	public bool IsRunning { get; set; }
	
	public virtual void Start()
	{
		this.rigidBody = this.GetComponent<Rigidbody>();
	}
	
	public override void Update()
	{
		base.Update();
		
		if(this.startPos != null)
		{
			this.transform.position = (Vector3)this.startPos;
			this.startPos = null;
		}
		
		if (this.movementState == CharacterMovementState.Running || this.movementState == CharacterMovementState.Standing || this.movementState == CharacterMovementState.Walking)
		{
			this.lastGroundedPosition = this.transform.position;
		}
		
		if(this.movementState == CharacterMovementState.Jumping || this.movementState == CharacterMovementState.Falling || this.movementState == CharacterMovementState.Leveling)
		{
			this.UpdateAirState();
		}
		else if (this.movementState != CharacterMovementState.Falling)
		{
			if(this.rigidBody.velocity.y < -0.01f)
			{
				print ("Falling");
				this.movementState = CharacterMovementState.Falling;
			}
		}	
	}
	
	public void Initialize(Vector3 position)
	{
		this.startPos = position;
	}
	
	public virtual void ForceDeath()
	{
	}
		
	// ---------------------------------------------
	// Protected
	// ---------------------------------------------	
	protected void StartJump()
	{
		// No jumping if we already are or have no rigid body\\ t
		if(this.movementState == CharacterMovementState.Jumping || this.movementState == CharacterMovementState.Falling || this.movementState == CharacterMovementState.Leveling || this.rigidBody == null)
		{
			return;
		}
		
		print ("Jumping");
		this.lastGroundedPosition = this.transform.position;
		this.rigidBody.AddForce(0, this.JumpStrength, 0);
		this.movementState = CharacterMovementState.Jumping;
		this.levelingTimer = 0;
	}
	
	protected void MoveCharacter(float newX, float newZ)
	{
		if(this.CanMoveInAir || this.movementState == CharacterMovementState.Running || this.movementState == CharacterMovementState.Standing || this.movementState == CharacterMovementState.Walking)
		{
			if(this.IsRunning)
			{
				print ("Running");
				if(this.movementState == CharacterMovementState.Standing || this.movementState == CharacterMovementState.Walking)
				{
					this.movementState = CharacterMovementState.Running;
				}
				
				transform.Translate(new Vector3(newX * this.RunSpeed, 0, newZ * this.RunSpeed), null);
			}
			else
			{
				print ("Walking");
				if(this.movementState == CharacterMovementState.Standing || this.movementState == CharacterMovementState.Running)
				{
					this.movementState = CharacterMovementState.Walking;
				}
				
				transform.Translate(new Vector3(newX * this.Speed, 0, newZ * this.Speed), null);
			}		
			
			this.moveDirection = new Vector2(newX, newZ);
		}
	}
	
	private void UpdateAirState()
	{
		transform.Translate(new Vector3(this.moveDirection.x * AirSpeed, 0, this.moveDirection.y * AirSpeed), null);
		
		if(this.transform.position.y < DeathDepth)
		{
			this.ForceDeath();
			this.transform.position = this.lastGroundedPosition;
			return;
		}
		
		if(this.rigidBody.velocity.y < 0.01f && this.rigidBody.velocity.y > -0.01f)
		{
			this.levelingTimer++;
			if(this.levelingTimer > 5)
			{
				print ("Standing");
				this.movementState = CharacterMovementState.Standing;
				return;
			} 
			
			if(this.movementState == CharacterMovementState.Jumping)
			{
				print ("Leveling");
				this.movementState = CharacterMovementState.Leveling;
			}
		}
	}
}
