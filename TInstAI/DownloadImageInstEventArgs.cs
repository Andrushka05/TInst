using System;
using System.Collections.Generic;

namespace TInstAI
{
    internal class DownloadImageInstEventArgs : EventArgs
    {
        private readonly List<ImageInst> m_imageInsts;

        public DownloadImageInstEventArgs(List<ImageInst> imageInsts)
        {
            m_imageInsts = imageInsts;
        }

        public List<ImageInst> ImageInsts { get { return m_imageInsts; } }
    }
}
