////////////////////////////////////////////////////////////////////////////////
// StickyWindows
//
// Copyright (c) 2009 Riccardo Pietrucci, 2017 Thomas Freudenberg
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the author be held liable for any damages arising from
// the use of this software.
// Permission to use, copy, modify, distribute and sell this software for any
// purpose is hereby granted without fee, provided that the above copyright
// notice appear in all copies and that both that copyright notice and this
// permission notice appear in supporting documentation.
//
//////////////////////////////////////////////////////////////////////////////////


using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StickyWindows
{
    public abstract class BaseFormAdapter
    {
        private Rectangle _extendedFrameMargin;

        public abstract IntPtr Handle { get; }
        protected abstract Rectangle InternalBounds { get; set; }
        public abstract Size MaximumSize { get; set; }
        public abstract Size MinimumSize { get; set; }
        public abstract bool Capture { get; set; }
        public abstract void Activate();
        public abstract Point PointToScreen(Point point);

        private void CaclulateExtendedFrameMargin()
        {
            if (Win32.DwmGetWindowAttribute(Handle, Win32.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out var rect, Marshal.SizeOf(typeof(Win32.RECT))) == 0)
            {
                var originalFormBounds = InternalBounds;
                _extendedFrameMargin = new Rectangle(
                    -(originalFormBounds.Left - rect.left),
                    -(originalFormBounds.Top - rect.top),
                    -(originalFormBounds.Width - (rect.right - rect.left)),
                    -(originalFormBounds.Height - (rect.bottom - rect.top)));
            }
            else
            {
                _extendedFrameMargin = Rectangle.Empty;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                CaclulateExtendedFrameMargin();

                var bounds = InternalBounds;
                bounds.X += _extendedFrameMargin.Left;
                bounds.Y += _extendedFrameMargin.Top;
                bounds.Width += _extendedFrameMargin.Width;
                bounds.Height += _extendedFrameMargin.Height;

                return bounds;
            }
            set
            {
                CaclulateExtendedFrameMargin();

                var bounds = value;
                bounds.X -= _extendedFrameMargin.Left;
                bounds.Y -= _extendedFrameMargin.Top;
                bounds.Width -= _extendedFrameMargin.Width;
                bounds.Height -= _extendedFrameMargin.Height;
                InternalBounds = bounds;
            }
        }
    }
}