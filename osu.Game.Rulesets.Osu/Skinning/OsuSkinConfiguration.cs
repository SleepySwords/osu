// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Rulesets.Osu.Skinning
{
    public enum OsuSkinConfiguration
    {
        SliderBorderSize,
        SliderPathRadius,
        CursorCentre,
        CursorExpand,
        CursorRotate,
        HitCircleOverlayAboveNumber,

        // ReSharper disable once IdentifierTypo
        HitCircleOverlayAboveNumer, // Some old skins will have this typo
        SpinnerFrequencyModulate,
        SpinnerNoBlink
    }
}
