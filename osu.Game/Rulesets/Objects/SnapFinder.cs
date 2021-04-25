// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Used to find the lowest beat divisor that a <see cref="HitObject"/> aligns to in an <see cref="IBeatmap"/>
    /// </summary>
    public class SnapFinder
    {
        private readonly IBeatmap beatmap;

        /// <summary>
        /// Creates a new SnapFinder instance.
        /// </summary>
        /// <param name="beatmap">The beatmap to align to when evaulating.</param>
        public SnapFinder(IBeatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        private readonly static int[] snaps = { 1, 2, 3, 4, 6, 8, 12, 16 };

        /// <summary>
        /// Finds the lowest beat divisor that the given HitObject aligns to.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to evaluate.</param>
        public int FindSnap(HitObject hitObject)
        {
            TimingControlPoint currentTimingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            double snapResult = (hitObject.StartTime - currentTimingPoint.Time) % (currentTimingPoint.BeatLength * 4);

            foreach (var snap in snaps)
            {
                if (almostDivisibleBy(snapResult, currentTimingPoint.BeatLength / (double)snap))
                    return snap;
            }

            return 0;
        }

        private const double leniency_ms = 1.0;

        private static bool almostDivisibleBy(double dividend, double divisor)
        {
            double remainder = Math.Abs(dividend) % divisor;
            return Precision.AlmostEquals(remainder, 0, leniency_ms) || Precision.AlmostEquals(remainder - divisor, 0, leniency_ms);
        }
    }
}
