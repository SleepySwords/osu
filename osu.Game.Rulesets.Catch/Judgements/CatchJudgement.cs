﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        /// <summary>
        /// Whether fruit on the platter should explode or drop.
        /// Note that this is only checked if the owning object is also <see cref="IHasComboInformation.LastInCombo" />
        /// </summary>
        public virtual bool ShouldExplodeFor(JudgementResult result) => result.IsHit;
    }
}
