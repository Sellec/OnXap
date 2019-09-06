using System.Collections.Generic;

namespace OnXap.Design.Additionals.ClassicStructures
{
    public class PhotoList : Dictionary<string, PhotoList.Photo>
    {
        public class Photo
        {
            public string preview_file;
            public string main_file;
        }

        public Photo this[int index]
        {
            get => this[index.ToString()];
            set => this[index.ToString()] = value;
        }
    }
}
