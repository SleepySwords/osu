// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Handlers.Trackpad;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TrackpadAreaSelection : CompositeDrawable
    {
        private readonly TrackpadHandler handler;

        private Container tabletContainer;
        private Container usableAreaContainer;

        private readonly Bindable<Vector2> areaOffset = new Bindable<Vector2>();
        private readonly Bindable<Vector2> areaSize = new Bindable<Vector2>();

        private Box usableFill;
        private OsuSpriteText usableAreaText;

        public TrackpadAreaSelection(TrackpadHandler handler)
        {
            this.handler = handler;

            Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = tabletContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 5,
                BorderThickness = 2,
                BorderColour = colour.Red,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colour.Gray1,
                    },
                    usableAreaContainer = new Container
                    {
                        BorderColour = colour.Blue,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            usableFill = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.6f,
                            },
                            new Box
                            {
                                Colour = Color4.White,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Height = 5,
                            },
                            new Box
                            {
                                Colour = Color4.White,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = 5,
                            },
                            usableAreaText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.White,
                                Font = OsuFont.Default.With(size: 12),
                                Y = 10
                            }
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                usableAreaContainer.MoveTo(val.NewValue * 200, 100, Easing.OutQuint)
                                   .OnComplete(_ => checkBounds()); // required as we are using SSDQ.
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(val.NewValue * 200, 100, Easing.OutQuint)
                                   .OnComplete(_ => checkBounds()); // required as we are using SSDQ.

                int x = (int)(val.NewValue.X * 200);
                int y = (int)(val.NewValue.Y * 200);
                int commonDivider = greatestCommonDivider(x, y);

                usableAreaText.Text = $"{(float)x / commonDivider}:{(float)y / commonDivider}";
            }, true);

            tabletContainer.Size = new Vector2(200, 200);
            checkBounds();
            // initial animation should be instant.
            FinishTransforms(true);
        }

        private static int greatestCommonDivider(int a, int b)
        {
            while (b != 0)
            {
                int remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }

        [Resolved]
        private OsuColour colour { get; set; }

        private void checkBounds()
        {
            var usableSsdq = usableAreaContainer.ScreenSpaceDrawQuad;

            bool isWithinBounds = tabletContainer.ScreenSpaceDrawQuad.Contains(usableSsdq.TopLeft + new Vector2(1)) &&
                                  tabletContainer.ScreenSpaceDrawQuad.Contains(usableSsdq.BottomRight - new Vector2(1));

            usableFill.FadeColour(isWithinBounds ? colour.Blue : colour.RedLight, 100);
        }

        protected override void Update()
        {
            base.Update();

            Vector2 size = new Vector2(200, 100);

            float maxDimension = size.LengthFast;

            float fitX = maxDimension / (DrawWidth - Padding.Left - Padding.Right);
            float fitY = maxDimension / DrawHeight;

            float adjust = MathF.Max(fitX, fitY);

            tabletContainer.Scale = new Vector2(1 / adjust);
        }
    }
}
