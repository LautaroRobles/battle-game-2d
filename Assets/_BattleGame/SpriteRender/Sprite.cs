using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class Sprite : MonoBehaviour
{
    public Material SpriteMaterial;
    public Texture2D SpriteSheet;
    public int SpriteWidth;
    public int SpriteHeight;
    public bool SpriteFilp;
    public int EditorFrame;
    private Dictionary<string, Animation> Animations;
    private struct Animation
    {
        public int Start;
        public int End;
        public float FrameRate;
        public bool Loop;
        public Animation(int start, int end, float frameRate, bool loop)
        {
            Start = start;
            End = end;
            FrameRate = frameRate;
            Loop = loop;
        }
    }
    private string AnimationToPlay;
    private int AnimationLoopCount;
    private int PreviousFrame = -1;
    private float FrameTimeStart;
    // Events
    public event Action<string> OnPlay;
    public event Action<string> OnStop;
    public event Action<string> OnLoop;
    void Start()
    {
        GetMaterial();
    }
    Material GetMaterial()
    {
        // if in editor mode
        if (!Application.IsPlaying(gameObject))
        {
            var sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
            if (sharedMaterial == null)
            {
                sharedMaterial = new Material(SpriteMaterial);
                GetComponent<MeshRenderer>().sharedMaterial = sharedMaterial;
            }

            var instanceID = sharedMaterial.GetInstanceID();

            return GetComponent<MeshRenderer>().sharedMaterial;
        }
        else
        {
            return GetComponent<MeshRenderer>().material;
        }
    }

    /// <summary>
    /// Generates animation data, then to play an animation use Play(name)
    /// </summary>
    /// <param name="name">Name of animation</param>
    /// <param name="start">Start frame</param>
    /// <param name="end">End frame</param>
    /// <param name="frameRate">Frame rate in fps (1 = 1 second each frame)</param>
    /// <param name="loop">Should loop</param>
    public void GenerateAnimation(string name, int start, int end, float frameRate, bool loop)
    {
        if (Animations == null)
            Animations = new Dictionary<string, Animation>();

        Animation animation = new Animation(start, end, frameRate, loop);

        Animations.Add(name, animation);
    }
    /// <summary>
    /// Plays animation by given name, should be called after GenerateAnimation()
    /// </summary>
    /// <param name="name">Name of animation to play</param>
    public void Play(string name)
    {
        AnimationToPlay = name;
        AnimationLoopCount = 0;
        FrameTimeStart = Time.time;

        OnPlay?.Invoke(name);
    }
    public void Stop()
    {
        OnStop?.Invoke(AnimationToPlay);

        AnimationToPlay = "";
    }

    private void RenderFrame(int frame)
    {
        Material material = GetMaterial();

        material.SetTexture("_SpriteSheet", SpriteSheet);
        material.SetFloat("_SpriteHeight", SpriteHeight);
        material.SetFloat("_SpriteWidth", SpriteWidth);
        material.SetInt("_SpriteFrame", frame);
        material.SetInt("_SpriteFlip", SpriteFilp ? -1 : 1);
    }

    void Update()
    {
        // If in editor
        if (!Application.IsPlaying(gameObject))
        {
            RenderFrame(EditorFrame);
            return;
        }

        if (Animations == null)
            return;

        if (AnimationToPlay == "" || AnimationToPlay == null)
            return;

        if (!Animations.ContainsKey(AnimationToPlay))
            return;

        if (!Animations[AnimationToPlay].Loop && AnimationLoopCount > 0)
            return;

        Animation animationToPlay = Animations[AnimationToPlay];

        int frameToPlay = Mathf.FloorToInt((Time.time - FrameTimeStart) / animationToPlay.FrameRate) % (animationToPlay.End - animationToPlay.Start + 1) + animationToPlay.Start;

        if (PreviousFrame != frameToPlay)
        {
            if (PreviousFrame == (animationToPlay.End) && frameToPlay == animationToPlay.Start)
            {
                AnimationLoopCount++;
                OnLoop?.Invoke(AnimationToPlay);
            }

            RenderFrame(frameToPlay);
            PreviousFrame = frameToPlay;
        }
    }
}
