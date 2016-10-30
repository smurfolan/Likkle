using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Likkle.DataModel.Repositories;

namespace Likkle.DataModel.UnitOfWork
{
    public class LikkleUoW
    {
        private readonly LikkleDbContext _dbContext = new LikkleDbContext();

        private AreaRepository areaRepository;

        public AreaRepository AreaRepository
        {
            get
            {
                if (areaRepository == null)
                    return new AreaRepository(_dbContext);

                return areaRepository;
            }
        }
    }
}
