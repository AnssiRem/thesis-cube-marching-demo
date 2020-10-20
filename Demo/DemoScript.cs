using UnityEngine;
using System.Collections;

public class DemoScript : MonoBehaviour
{
    private Chunk[] chunks;

    [SerializeField] private Vector3 levelSize = Vector3.one * 10f;
    [SerializeField] private Vector3Int chunkGrid = Vector3Int.one * 4;
    [SerializeField] private Vector3Int voxelGrid = Vector3Int.one * 10;
    [SerializeField] private int seed = 0;
    [SerializeField] [Range(-1f, 1f)] private float surface = 0.5f;

    private void Start()
    {
        LevelGeneration.GenerateXYZ(levelSize, chunkGrid, voxelGrid, surface, seed);
    }
}
