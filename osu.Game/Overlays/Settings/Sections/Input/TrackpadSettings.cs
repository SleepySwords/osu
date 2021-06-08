// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Trackpad;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osuTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TrackpadSettings : SettingsSubsection
    {
        protected override string Header => "Trackpad";

        private readonly TrackpadHandler handler;

        private readonly Bindable<Vector2> areaOffset = new Bindable<Vector2>();
        private readonly Bindable<Vector2> areaSize = new Bindable<Vector2>();

        private readonly BindableNumber<float> offsetX = new BindableNumber<float> { MinValue = 0, Precision = 0.01f, MaxValue = 1 };
        private readonly BindableNumber<float> offsetY = new BindableNumber<float> { MinValue = 0, Precision = 0.01f, MaxValue = 1 };

        private readonly BindableNumber<float> sizeX = new BindableNumber<float> { MinValue = 0.05f, Precision = 0.01f, MaxValue = 1 };
        private readonly BindableNumber<float> sizeY = new BindableNumber<float> { MinValue = 0.05f, Precision = 0.01f, MaxValue = 1 };

        private readonly BindableBool aspectLock = new BindableBool();

        private ScheduledDelegate aspectRatioApplication;

        /// <summary>
        /// Based on ultrawide monitor configurations.
        /// </summary>
        private const float largest_feasible_aspect_ratio = 21f / 9;

        private readonly BindableNumber<float> aspectRatio = new BindableFloat(1)
        {
            MinValue = 1 / largest_feasible_aspect_ratio,
            MaxValue = largest_feasible_aspect_ratio,
            Precision = 0.01f,
        };

        public TrackpadSettings(TrackpadHandler handler)
        {
            this.handler = handler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new TrackpadAreaSelection(handler)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 300,
                },
                new SettingsCheckbox
                {
                    LabelText = "Enabled",
                    Current = handler.Enabled
                },
                new SensitivitySetting
                {
                    LabelText = "X Offset",
                    Current = offsetX
                },
                new SensitivitySetting
                {
                    LabelText = "Y Offset",
                    Current = offsetY
                },
                new SensitivitySetting
                {
                    LabelText = "Aspect Ratio",
                    Current = aspectRatio
                },
                new SettingsCheckbox
                {
                    LabelText = "Lock Aspect Ratio",
                    Current = aspectLock
                },
                new SensitivitySetting
                {
                    LabelText = "Width",
                    Current = sizeX
                },
                new SensitivitySetting
                {
                    LabelText = "Height",
                    Current = sizeY
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                offsetX.Value = val.NewValue.X;
                offsetY.Value = val.NewValue.Y;
            }, true);

            offsetX.BindValueChanged(val => areaOffset.Value = new Vector2(val.NewValue, areaOffset.Value.Y));
            offsetY.BindValueChanged(val => areaOffset.Value = new Vector2(areaOffset.Value.X, val.NewValue));

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                sizeX.Value = val.NewValue.X;
                sizeY.Value = val.NewValue.Y;
            }, true);

            sizeX.BindValueChanged(val =>
            {
                areaSize.Value = new Vector2(val.NewValue, areaSize.Value.Y);

                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => applyAspectRatio(sizeX));
            });

            sizeY.BindValueChanged(val =>
            {
                areaSize.Value = new Vector2(areaSize.Value.X, val.NewValue);

                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => applyAspectRatio(sizeY));
            });

            updateAspectRatio();
            aspectRatio.BindValueChanged(aspect =>
            {
                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => forceAspectRatio(aspect.NewValue));
            });

            offsetX.Default = 0.5f;
            offsetY.Default = 0.5f;

            sizeX.Default = 0.9f;
            sizeY.Default = 0.9f;
        }



        private void applyAspectRatio(BindableNumber<float> sizeChanged)
        {
            try
            {
                if (!aspectLock.Value)
                {
                    float proposedAspectRatio = currentAspectRatio;

                    if (proposedAspectRatio >= aspectRatio.MinValue && proposedAspectRatio <= aspectRatio.MaxValue)
                    {
                        // aspect ratio was in a valid range.
                        updateAspectRatio();
                        return;
                    }
                }

                // if lock is applied (or the specified values were out of range) aim to adjust the axis the user was not adjusting to conform.
                if (sizeChanged == sizeX)
                    sizeY.Value = (int)(areaSize.Value.X / aspectRatio.Value);
                else
                    sizeX.Value = (int)(areaSize.Value.Y * aspectRatio.Value);
            }
            finally
            {
                // cancel any event which may have fired while updating variables as a result of aspect ratio limitations.
                // this avoids a potential feedback loop.
                aspectRatioApplication?.Cancel();
            }
        }

        private void forceAspectRatio(float aspectRatio)
        {
            aspectLock.Value = false;

            int proposedHeight = (int)(sizeX.Value / aspectRatio);

            if (proposedHeight < sizeY.MaxValue)
                sizeY.Value = proposedHeight;
            else
                sizeX.Value = (int)(sizeY.Value * aspectRatio);

            updateAspectRatio();

            aspectRatioApplication?.Cancel();
            aspectLock.Value = true;
        }

        private void updateAspectRatio() => aspectRatio.Value = currentAspectRatio;

        private float currentAspectRatio => sizeX.Value / sizeY.Value;

        private class SensitivitySetting : SettingsSlider<float, SensitivitySlider>
        {
            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        private class SensitivitySlider : OsuSliderBar<float>
        {
            public override string TooltipText => Current.Disabled ? "enable high precision mouse to adjust sensitivity" : $"{base.TooltipText}x";
        }
    }
}
