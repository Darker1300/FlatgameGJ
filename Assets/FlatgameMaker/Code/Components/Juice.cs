using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Juice : MonoBehaviour
{

    public bool randomizeStarts;
    //spin
    [Header("Spin: Spins the object (at turnspeed degrees per second)")]
    public bool spin = false;
    [ConditionalField("spin", true)]
    public float turnSpeed = 180;

    //sine
    [Header("Sine Transform: modifies the transform values based on sine waves, speed is cycles per second")]
    public bool sineTransform = false;
    [ConditionalField("sineTransform", true)]
    public bool sineMove = false;
    [ConditionalField("sineTransform", true)]
    public Vector3 moveDirection = Vector3.up;
    [ConditionalField("sineTransform", true)]
    public float moveSpeed;
    [ConditionalField("sineTransform", true)]
    public bool sineRotation = false;
    [ConditionalField("sineTransform", true)]
    public float rotationDegrees;
    [ConditionalField("sineTransform", true)]
    public float rotationSpeed;
    [ConditionalField("sineTransform", true)]
    public bool sineScale = false;
    [ConditionalField("sineTransform", true)]
    public Vector3 scaleAmount = Vector3.one;
    [ConditionalField("sineTransform", true)]
    public float scaleSpeed;

    //shaker
    [Header("Shake: Shakes the object set FPS to 0 for it to update every frame")]
    public bool shake = false;
    [ConditionalField("shake", true)]
    public float shakeSpeed = 20;
    [ConditionalField("shake", true)]
    public float shakeMagnitude = 1;
    [ConditionalField("shake", true)]
    public float shakeFPS = 0;

    //move with camera
    [Header("Move With Camera: Moves based on the camera position, percent speeds determines how that movement is scaled")]
    public bool moveWithCamera = false;
    [ConditionalField("moveWithCamera", true)]
    public Vector2 percentSpeeds = Vector2.one * 100;
    [ConditionalField("moveWithCamera", true)]
    public bool moveOutsideArea = false;

    //controlled spin
    [Header("Controlled Spin: Like spin but controlled by user input")]
    public bool controlledSpin = false;
    [ConditionalField("controlledSpin", true)]
    public bool useUnityAxisInput = false;
    [ConditionalField("controlledSpin", true)]
    public bool smoothSpin = false;
    [ConditionalField("controlledSpin", true)]
    public string spinInputAxis = "Horizontal";
    [ConditionalField("controlledSpin", true)]
    public KeyCode clockwiseKey = KeyCode.Period;
    [ConditionalField("controlledSpin", true)]
    public KeyCode antiClockwiseKey = KeyCode.Comma;
    [ConditionalField("controlledSpin", true)]
    public float controlledTurnSpeed = 180;
    [ConditionalField("controlledSpin", true)]
    public float spinSmoothingAcceleration = 10;

    //transform data
    Vector3 startPos;
    Vector3 startRot;
    Vector3 startScale;

    Vector3 currentPos;
    Vector3 currentRot;
    Vector3 currentScale;

    //spin data
    float spinRotation = 0;

    //controlled spin data
    float spinOffset = 0;
    float smoothedSpinAmount = 0;

    //sine data
    float twoPI = Mathf.PI* 2;
    float moveTime = 0;
    float rotationTime = 0;
    float scaleTime = 0;

    //shaker data
    float shakeFrameLength = 0;
    float updateTimer = 0;
    float shakeSamplePos = 0;

    //move with camera data
    Transform mainCamera;
    Vector3 cameraStartPosition;
    AreaManager manager;
    int objectArea;
    Vector3 cameraMoveOffset;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localEulerAngles;
        startScale = transform.localScale;
        if (shakeFPS > 0)
        {
            shakeFrameLength = 1 / shakeFPS;
        }
        if (randomizeStarts)
        {
            if (sineTransform && sineMove)
            {
                moveTime = Random.value;
            }
            if (sineTransform && sineRotation)
            {
                rotationTime = Random.value;
            }
            if (sineTransform && sineScale)
            {
                scaleTime = Random.value;
            }
            if (spin)
            {
                spinRotation = Random.Range(0, 360);
            }
        }
        shakeSamplePos = Random.value;
        mainCamera = Camera.main.transform;
        manager = AreaManager.Instance;
        objectArea = manager.IndexOfArea(manager.FindAreaOf(transform));
        cameraStartPosition = manager.FindAreaOf(transform).AreaPlayer.transform.position;
    }

    void Update()
    {
        currentPos = startPos;
        currentRot = startRot;
        currentScale = startScale;

        Spin();
        ControlledSpin();
        SineMovement();
        Shake();
        MoveWithCamera();

        transform.localPosition = currentPos;
        transform.localEulerAngles = currentRot;
        transform.localScale = currentScale;
    }

    void Spin()
    {
        if (spin)
        {
            spinRotation += turnSpeed * Time.deltaTime;
        }
        currentRot += Vector3.forward * spinRotation;
    }

    void ControlledSpin()
    {
        if (controlledSpin)
        {
            float spinAmount = 0;
            if (useUnityAxisInput)
            {
                spinAmount = Input.GetAxis(spinInputAxis);
            }
            else
            {
                if (Input.GetKey(clockwiseKey))
                {
                    spinAmount = 1;
                }
                else if (Input.GetKey(antiClockwiseKey))
                {
                    spinAmount = -1;
                }
            }
            if (smoothSpin)
            {
                smoothedSpinAmount = Mathf.MoveTowards(smoothedSpinAmount, spinAmount, Time.deltaTime * spinSmoothingAcceleration);
                spinOffset += smoothedSpinAmount * Time.deltaTime * controlledTurnSpeed;
            }
            else
            {
                spinOffset += spinAmount * Time.deltaTime * controlledTurnSpeed;
            }
        }
        currentRot += Vector3.forward * spinOffset;
    }

    void SineMovement()
    {
        if (sineTransform)
        {
            if (sineMove)
            {
                moveTime += moveSpeed * Time.deltaTime;
                currentPos += moveDirection * Mathf.Sin(moveTime * twoPI);
            }
            if (sineRotation)
            {
                rotationTime += rotationSpeed * Time.deltaTime;
                currentRot += Vector3.forward * rotationDegrees * Mathf.Sin(rotationTime * twoPI);
            }
            if (sineScale)
            {
                scaleTime += scaleSpeed * Time.deltaTime;
                currentScale = Vector3.Scale(startScale, Vector3.one + (scaleAmount * Mathf.Sin(scaleTime * twoPI)));
            }
        }

    }

    void Shake()
    {
        if (shake)
        {
            updateTimer += Time.deltaTime;
            if (shakeFPS == 0) 
            {
                shakeSamplePos += shakeSpeed * Time.deltaTime;
            }
            else if( updateTimer > shakeFrameLength)
            {
                shakeSamplePos += shakeSpeed * (int)(updateTimer / shakeFrameLength);
                updateTimer = updateTimer % shakeFrameLength;
            }
            currentPos += shakeMagnitude * new Vector3(Mathf.PerlinNoise(shakeSamplePos, 0) * 2 - 1, Mathf.PerlinNoise(0, shakeSamplePos) * 2 - 1);
        }
        

    }

    void MoveWithCamera()
    {
        if (moveWithCamera)
        {
            if(moveOutsideArea || objectArea == manager.currentArea)
            {
                cameraMoveOffset = Vector3.Scale((mainCamera.position - cameraStartPosition), percentSpeeds/100);
            }
        }
        currentPos += cameraMoveOffset;
    }
}
