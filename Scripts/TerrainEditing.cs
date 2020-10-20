using System;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainEditing
{
    // TODO: Finish implementing a task system to resolve framerate chugging because calculations
    // are being called from the Update().
    private struct EditTask
    {
        public Chunk Chunk;
        public Vector3 HitPos;
        public float Radius;
        public bool IsSubtract;
        public bool IsGradient;
    }

    private static List<EditTask> editTaskBuffer = new List<EditTask>();
    private static List<EditTask> startedTasks = new List<EditTask>();

    private static bool isPerformingTask = false;

    public static void CreateEditTask(Chunk chunk, Vector3 hitPos, float radius, bool isSubtract = true,
        bool isGradient = false)
    {
        //editTaskBuffer.Add(new EditTask()
        //{
        //    Chunk = chunk,
        //    HitPos = hitPos,
        //    Radius = radius,
        //    IsSubtract = isSubtract,
        //    IsGradient = isGradient
        //});

        PerformEditTask(new EditTask()
        {
            Chunk = chunk,
            HitPos = hitPos,
            Radius = radius,
            IsSubtract = isSubtract,
            IsGradient = isGradient
        });
    }

    private static void PerformEditTask(EditTask t)
    {
        isPerformingTask = true;

        Vector3 hitLocalPos = t.HitPos - t.Chunk.transform.position;

        for (int k = 0; k < t.Chunk.Size.z; k++)
        {
            for (int j = 0; j < t.Chunk.Size.y; j++)
            {
                for (int i = 0; i < t.Chunk.Size.x; i++)
                {
                    int index = i + j * t.Chunk.Size.x + k * t.Chunk.Size.x * t.Chunk.Size.y;

                    Vector3 voxelPos = new Vector3(
                        i * LevelGeneration.VoxelSize.x,
                        j * LevelGeneration.VoxelSize.y,
                        k * LevelGeneration.VoxelSize.z);

                    float dist = (hitLocalPos - voxelPos).magnitude;
                    if (dist <= t.Radius)
                    {
                        float value;
                        if (t.IsGradient)
                        {
                            value = t.Chunk.Voxels[index] +
                                Mathf.Lerp(-1 + 2 * Convert.ToInt32(t.IsSubtract),
                                0, dist / t.Radius);
                        }
                        else
                        {
                            value = t.Chunk.Voxels[index] - 1 + 2 * Convert.ToInt32(t.IsSubtract);
                        }

                        t.Chunk.Voxels[index] = Mathf.Clamp(value, -1, 1);
                    }
                }
            }
        }

        t.Chunk.CubeMarch();
    }
}
