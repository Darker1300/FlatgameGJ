using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class Player : MonoBehaviour
{

    public float movementSpeed = 3;

    public bool normaliseMovement = false;

    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public bool useUnityAxisInput = false;
    public string verticalAxis = "Vertical";
    public string horizontalAxis = "Horizontal";


    [FormerlySerializedAs("space")]
    Area area;
    Rect bounds;
    float boundsOffset;
    AreaBoundsCollision boundsCollision;


    private void Update()
    {
		UpdateSettings();
		UpdateMovement();
    }
	
	private  void UpdateMovement()
	{		
        var direction = Vector3.zero;
        if (useUnityAxisInput)
        {
            direction = new Vector3(
                Input.GetAxisRaw(horizontalAxis), 
                Input.GetAxisRaw(verticalAxis) );
        }
        else
        {
            if (Input.GetKey(leftKey))
            {
                direction += Vector3.left;
            }

            if (Input.GetKey(rightKey))
            {
                direction += Vector3.right;
            }

            if (Input.GetKey(upKey))
            {
                direction += Vector3.up;
            }

            if (Input.GetKey(downKey))
            {
                direction += Vector3.down;
            }
        }


        if (normaliseMovement)
        {
            direction = Vector3.ClampMagnitude(direction, 1.0f);
        }

        // Time.deltaTime tells us how many seconds have passed since we last
        // updated the movement - we multiply by movementSpeed to work out
        // how far that many seconds of movement takes us, and multiply by
        // direction to work out the final vector of movement
        Vector2 displacement = direction * movementSpeed * Time.deltaTime;
        Vector2 OutcomePos = (Vector2)transform.position + displacement;
        switch (boundsCollision)
        {
            case AreaBoundsCollision.Hard:
                OutcomePos.x = Mathf.Max(Mathf.Min(OutcomePos.x, bounds.xMax - boundsOffset), bounds.xMin + boundsOffset);
                OutcomePos.y = Mathf.Max(Mathf.Min(OutcomePos.y, bounds.yMax - boundsOffset), bounds.yMin + boundsOffset);
                
                break;
            case AreaBoundsCollision.Soft:
                float multValX = Mathf.Clamp01((Mathf.Abs(OutcomePos.x) - (bounds.xMax - boundsOffset))/boundsOffset);
                if(Mathf.Sign(displacement.x) == Mathf.Sign(OutcomePos.x))
                {
                    OutcomePos.x = Mathf.Lerp(OutcomePos.x, transform.position.x, multValX);
                }
                float multValY = Mathf.Clamp01((Mathf.Abs(OutcomePos.y) - (bounds.yMax - boundsOffset)) / boundsOffset);
                if (Mathf.Sign(displacement.y) == Mathf.Sign(OutcomePos.y))
                {
                    OutcomePos.y = Mathf.Lerp(OutcomePos.y, transform.position.y, multValY);
                }
                break;
            /*case SpaceBoundsCollision.Elastic:
                float elasticX = Mathf.Clamp01((Mathf.Abs(OutcomePos.x) - (bounds.xMax - boundsOffset)) / boundsOffset);
                OutcomePos.x += -Mathf.Sign(OutcomePos.x - bounds.x) * movementSpeed * elasticX * Time.deltaTime;
                float elasticY = Mathf.Clamp01((Mathf.Abs(OutcomePos.y) - (bounds.yMax - boundsOffset)) / boundsOffset);
                OutcomePos.y += -Mathf.Sign(OutcomePos.y - bounds.y) * movementSpeed * elasticY * Time.deltaTime;
                break;*/
            case AreaBoundsCollision.Loop:
                if(OutcomePos.x > bounds.xMax - boundsOffset)
                {
                    OutcomePos.x -= bounds.width - (2 * boundsOffset);
                }
                else if(OutcomePos.x < bounds.xMin + boundsOffset)
                {
                    OutcomePos.x += bounds.width - (2 * boundsOffset);
                }
                if (OutcomePos.y > bounds.yMax - boundsOffset)
                {
                    OutcomePos.y -= bounds.height - (2 * boundsOffset);
                }
                else if (OutcomePos.y < bounds.yMin + boundsOffset)
                {
                    OutcomePos.y += bounds.height - (2 * boundsOffset);
                }
                break;
        }
        displacement = OutcomePos - (Vector2)transform.position;
        transform.Translate(displacement, Space.World);
	}

    //The above code controls movement and 
    //the following code controls turning & flipping the object.

    [Header("")]
    public FlipWithMovementVariables flipWithMovement;
    public TurnWithMovementVariables turnWithMovement;
    public bool resetWhenStill;

    private Vector2 prevPosition;

    private void Awake()
    {
        if(flipWithMovement == null)
            flipWithMovement = new FlipWithMovementVariables();
        if(turnWithMovement == null)
            turnWithMovement = new TurnWithMovementVariables();

        prevPosition = transform.position;
        turnWithMovement.startAngle = transform.eulerAngles.z;

        turnWithMovement.enabledLastFrame = turnWithMovement.enabled;
        flipWithMovement.flipHorizontalLastFrame = flipWithMovement.flipHorizontal;
        flipWithMovement.flipVerticalLastFrame = flipWithMovement.flipVertical;
        area = AreaManager.Instance.FindAreaOf(transform);
        bounds = area.bounds;
        boundsOffset = area.boundsCollisionOffset;
        boundsCollision = area.boundsCollision;
    }

    private void LateUpdate()
    {
        UpdateFlipWithMovement();
        UpdateTurnWithMovement();
    }

    private void UpdateFlipWithMovement()
    {
        if (!flipWithMovement.flipHorizontal && !flipWithMovement.flipVertical)
            return;

        Vector2 nextPosition = transform.position;
        Vector2 delta = nextPosition - prevPosition;

        if (flipWithMovement.flipHorizontal)
        {
            if (delta.x != 0)
                SetFlippedHorizontal(delta.x < 0);
        }

        if (flipWithMovement.flipVertical)
        {
            if(delta.y != 0)
                SetFlippedVertical(delta.y < 0);
        }
        
        if(resetWhenStill && delta.magnitude < 0.0001f)
        {
            SetFlippedHorizontal(false);
            SetFlippedVertical(false);
        }


        prevPosition = nextPosition;
    }

    public void SetFlippedHorizontal(bool flipped)
    {
        if (flipWithMovement.flippedHorizontal != flipped)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

            flipWithMovement.flippedHorizontal = flipped;
        }
    }

    public void SetFlippedVertical(bool flipped)
    {
        if (flipWithMovement.flippedVertical != flipped)
        {
            Vector3 scale = transform.localScale;
            scale.y *= -1;
            transform.localScale = scale;

            flipWithMovement.flippedVertical = flipped;
        }
    }

    private void UpdateTurnWithMovement()
    {
        if (!turnWithMovement.enabled)
            return;

        Vector2 nextPosition = transform.position;
        Vector2 delta = nextPosition - prevPosition;
        Vector3 angles = transform.eulerAngles;
        float angle = transform.eulerAngles.z;

        float target; 

        if (delta.magnitude > 0.001f)
        {
            target = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        }
        else
        {
            if (resetWhenStill)
            {
                target = turnWithMovement.startAngle;
            }
            else
            {
                target = angle;
            }
        }

        angles.z = Mathf.SmoothDampAngle(angle,
                                         target,
                                         ref turnWithMovement.angleVelocity,
                                         turnWithMovement.rotateTime);
        transform.eulerAngles = angles;

        prevPosition = nextPosition;
    }



    private void UpdateSettings()
    {
        if (flipWithMovement.flipHorizontalLastFrame != flipWithMovement.flipHorizontal)
        {
            flipWithMovement.flipHorizontalLastFrame = flipWithMovement.flipHorizontal;

            turnWithMovement.enabledLastFrame = turnWithMovement.enabled = false;
        }

        if (flipWithMovement.flipVerticalLastFrame != flipWithMovement.flipVertical)
        {
            flipWithMovement.flipVerticalLastFrame = flipWithMovement.flipVertical;

            turnWithMovement.enabledLastFrame = turnWithMovement.enabled = false;
        }

        if (turnWithMovement.enabledLastFrame != turnWithMovement.enabled)
        {
            turnWithMovement.enabledLastFrame = turnWithMovement.enabled;

            flipWithMovement.flipHorizontal = flipWithMovement.flipHorizontalLastFrame = false;
            flipWithMovement.flipVertical = flipWithMovement.flipVerticalLastFrame = false;
        }
    }

}

[System.Serializable]
public class FlipWithMovementVariables
{
    public bool flipHorizontal;
    public bool flipVertical;

    [HideInInspector]
    public bool flippedHorizontal;
    [HideInInspector]
    public bool flippedVertical;

    [HideInInspector]
    public bool flipHorizontalLastFrame;
    [HideInInspector]
    public bool flipVerticalLastFrame;
}

[System.Serializable]
public class TurnWithMovementVariables
{
    public bool enabled;
    [HideInInspector]
    public bool enabledLastFrame;

    [Header("How long does turning take?")]
    public float rotateTime = 0.4f;

    [HideInInspector]
    public float startAngle;
    [HideInInspector]
    public float angleVelocity;
    [HideInInspector]
    public bool flipped;
}