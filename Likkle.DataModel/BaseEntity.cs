using System;

namespace Likkle.DataModel
{
    public class BaseEntity
    {
        private DateTime? dateCreated = null;

        public DateTime DateCreated
        {
            get { return dateCreated ?? DateTime.UtcNow; }
            set { dateCreated = value; }
        }
    }
}
