using UnityEngine;

public static class LevelGeneration
{
    private static int seed;
    private static float surface;
    private static Chunk[] chunks;
    private static Transform level;
    private static Vector3 chunkSize;
    private static Vector3 voxelSize;

    public static float Surface { get => surface; }
    public static Vector3 VoxelSize { get => voxelSize; }

    /// <summary>
    /// Generates a level divided into chunks along the X and Z axes.
    /// </summary>
    public static void GenerateXZ(
        Vector3 levelSize,
        Vector2Int chunkGridSize,
        Vector3Int voxelGridSize,
        float surface,
        int seed = 0)
    {
        GenerateXYZ(
            levelSize,
            new Vector3Int(chunkGridSize.x, 1, chunkGridSize.y),
            voxelGridSize,
            surface,
            seed);
    }


    /// <summary>
    /// Generates a level divided into chunks along all axes.
    /// </summary>
    public static void GenerateXYZ(
        Vector3 levelSize,
        Vector3Int chunkGridSize,
        Vector3Int voxelGridSize,
        float surface,
        int seed)
    {
        LevelGeneration.seed = seed;
        LevelGeneration.surface = surface;
        chunks = new Chunk[chunkGridSize.x * chunkGridSize.y * chunkGridSize.z];
        level = new GameObject("Level").transform;
        chunkSize = new Vector3(
            levelSize.x / chunkGridSize.x,
            levelSize.y / chunkGridSize.y,
            levelSize.z / chunkGridSize.z);
        voxelSize = new Vector3(
            chunkSize.x / voxelGridSize.x,
            chunkSize.y / voxelGridSize.y,
            chunkSize.z / voxelGridSize.z);

        Perlin.GeneratePerm(seed);

        for (int k = 0; k < chunkGridSize.z; k++)
        {
            for (int j = 0; j < chunkGridSize.y; j++)
            {
                for (int i = 0; i < chunkGridSize.x; i++)
                {
                    int index = i + j * chunkGridSize.x + k * chunkGridSize.x * chunkGridSize.y;

                    Transform nextChunk =
                        new GameObject("Chunk (" + i + ", " + j + ", " + k + ")").transform;
                    nextChunk.SetParent(level);
                    nextChunk.localPosition =
                        new Vector3(i * chunkSize.x, j * chunkSize.y, k * chunkSize.z);

                    chunks[index] = nextChunk.gameObject.AddComponent<Chunk>();
                    chunks[index].GenerateNoise(voxelGridSize, new Vector3Int(i, j, k), 1f, 3);
                    chunks[index].CubeMarch();
                }
            }
        }
    }
}
