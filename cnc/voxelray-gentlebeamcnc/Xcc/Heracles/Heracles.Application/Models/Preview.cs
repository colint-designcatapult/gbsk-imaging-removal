using Heracles.Core.Enums;
using Heracles.Core.Models;
using System.Windows.Media;

namespace Heracles.Application.Models
{
    public class Preview : IPreview
    {
        public ImageSource Source { get; set; }
        public string Path { get; set; }
        public string Modality { get; set; }
    }
}
