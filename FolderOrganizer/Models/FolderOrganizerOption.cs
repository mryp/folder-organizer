using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderOrganizer.Models
{
    public class FolderOrganizerOption
    {
        public enum DeleteType
        {
            None,
            HitDelete,
            OtherDelete,
        }

        public string SearchPattern
        {
            get;
            set;
        }

        public DeleteType SearchDeleteType
        {
            get;
            set;
        }

        public bool MoveUpFolder
        {
            get;
            set;
        }

        public FolderOrganizerOption()
        {
        }
    }
}
