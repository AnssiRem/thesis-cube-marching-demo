using UnityEngine;

public class Chunk : MonoBehaviour
{
    private float[] voxels;
    private Vector3Int size;

    public float[] Voxels { get => voxels; set => voxels = value; }
    public Vector3Int Size { get => size; }

    /// <summary>
    /// Populates the voxel array with values from the Perlin noise.
    /// </summary>
    public void GenerateNoise(Vector3Int voxelGrid, Vector3Int chunkPos, float freq, int octave)
    {
        size = voxelGrid + Vector3Int.one;
        voxels = new float[size.x * size.y * size.z];

        for (int k = 0; k < size.z; k++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    int index = i + j * size.x + k * size.x * size.y;

                    voxels[index] = Perlin.Fbm(
                        (float)i / (size.x - 1) + chunkPos.x,
                        (float)j / (size.y - 1) + chunkPos.y,
                        (float)k / (size.z - 1) + chunkPos.z,
                        octave);
                }
            }
        }
    }

    /// <summary>
    /// Creates a ground to the voxel array using values from the Perlin noise, above which the
    /// voxels values are faded away the higher they are from the ground.
    /// </summary>
    public void AddGround(float groundLevel, Vector3Int chunkPos, float freq, int octave)
    {
        for (int k = 0; k < size.z; k++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    int index = i + j * size.x + k * size.x * size.y;

                    float voxelHeight = j * LevelGeneration.VoxelSize.y + chunkPos.y;
                    float groundHeight = Perlin.Fbm(
                        (float)i / (size.x - 1) + chunkPos.x,
                        (float)k / (size.z - 1) + chunkPos.z,
                        3);

                    if (voxelHeight > groundLevel + groundHeight)
                    {
                        voxels[index] = Mathf.Clamp(
                            voxels[index] +
                            2 * (voxelHeight - groundLevel) /
                            ((size.y - 1) * LevelGeneration.VoxelSize.y - groundLevel),
                            -1f,
                            1f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds a box to the voxels using Unity Bounds.
    /// </summary>
    public void AddBox(Vector3 center, Vector3 boxSize, Vector3 chunkSize, float surface)
    {
        Bounds box = new Bounds(center, boxSize);

        for (int k = 0; k < size.z; k++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    Vector3 voxelPos = new Vector3(
                        (float)i / (size.x - 1) * chunkSize.x,
                        (float)j / (size.y - 1) * chunkSize.y,
                        (float)k / (size.z - 1) * chunkSize.z);

                    if (box.Contains(voxelPos))
                    {
                        int index = i + j * size.x + k * size.x * size.y;

                        voxels[index] = surface - 0.01f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds a really rough looking sphere to the voxels.
    /// </summary>
    public void AddSphere(Vector3 center, float radius, Vector3 chunkSize, float surface)
    {
        for (int k = 0; k < size.z; k++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    Vector3 voxelPos = new Vector3(
                        (float)i / (size.x - 1) * chunkSize.x,
                        (float)j / (size.y - 1) * chunkSize.y,
                        (float)k / (size.z - 1) * chunkSize.z);

                    float dist = (center - voxelPos).magnitude;
                    if (dist <= radius)
                    {
                        int index = i + j * size.x + k * size.x * size.y;

                        voxels[index] = surface - 0.01f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates meshes from the voxel array using cube marching.
    /// </summary>
    public void CubeMarch()
    {
        Mesh[] meshes = CubeMarching.March(voxels, size, LevelGeneration.VoxelSize,
            LevelGeneration.Surface);

        // Remove previous child meshes when editing.
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < meshes.Length; i++)
        {
            Transform nextMesh = new GameObject("Mesh " + i).transform;
            nextMesh.SetParent(transform, false);

            MeshFilter filter = nextMesh.gameObject.AddComponent<MeshFilter>();
            filter.mesh = meshes[i];

            MeshRenderer renderer = nextMesh.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));

            nextMesh.gameObject.AddComponent<MeshCollider>();
        }
    }
}
