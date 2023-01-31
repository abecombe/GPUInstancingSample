using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ParticleSimulation : MonoBehaviour
{
    struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
    }

    [SerializeField] protected int _numParticles;
    [SerializeField] protected ComputeShader _particleCS;

    protected ComputeBuffer _particleBuffer;
    const int SIMULATION_BLOCK_SIZE = 256;

    [Header("Simulation")]
    [SerializeField] protected Vector2 _acceleration = Vector2.down * 9.8f;
    [SerializeField, Range(0, 4)] protected float _drag = 2.5f;

    [Space()]
    [SerializeField] protected float _noiseAmplitude = 1f;
    [SerializeField] protected float _noisePositionScale = 1f;
    [SerializeField] protected float _noiseTimeScale = 1f;

    [Header("Rendering")]
    [SerializeField] protected Material _particleMat;
    [SerializeField] protected Texture2D _particleTex;
    [SerializeField] protected Color _color;
    [SerializeField] protected float _size;

    protected void InitBuffer()
    {
        _particleBuffer = new ComputeBuffer(_numParticles, Marshal.SizeOf(typeof(Particle)));
        var particleArray = new Particle[_numParticles];
        for (var i = 0; i < _numParticles; i++)
        {
            particleArray[i].Position = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            particleArray[i].Velocity = Vector2.zero;
        }
        _particleBuffer.SetData(particleArray);
    }

    protected void SimulateParticle()
    {
        var cs = _particleCS;

        int id = cs.FindKernel("Main");

        int threadGroupSize = Mathf.CeilToInt((float)_numParticles / SIMULATION_BLOCK_SIZE);

        cs.SetInt("_NumParticles", _numParticles);

        cs.SetFloat("_Time", Time.time);
        cs.SetFloat("_DeltaTime", Time.deltaTime);

        var drag = Mathf.Exp(-_drag * Time.deltaTime);
        Debug.Log(Time.deltaTime);
        var acceleration = new Vector3(_acceleration.x, _acceleration.y, drag);
        cs.SetVector("_Acceleration", acceleration);
        cs.SetVector("_NoiseParams", new Vector3(_noiseAmplitude, _noisePositionScale, _noiseTimeScale));

        cs.SetBuffer(id, "_ParticleBuffer", _particleBuffer);

        cs.Dispatch(id, threadGroupSize, 1, 1);
    }
    protected void RenderParticle()
    {
        _particleMat.SetBuffer("_ParticleBuffer", _particleBuffer);
        _particleMat.SetTexture("_ParticleTex", _particleTex);
        _particleMat.SetColor("_Color", _color);
        _particleMat.SetFloat("_Size", _size);
        Graphics.DrawProcedural(_particleMat, new Bounds(Vector3.zero, Vector3.one * 10000), MeshTopology.Points, _numParticles);
    }


    protected void ReleaseBuffer()
    {
        _particleBuffer.Dispose();
    }

    protected void Start()
    {
        InitBuffer();
    }

    protected void Update()
    {
        SimulateParticle();
        RenderParticle();
    }

    protected void OnDestroy()
    {
        ReleaseBuffer();
    }
}
