using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
{
    public class ClassGroupRepository : Repository<ClassGroup>, IClassGroupRepository
    {
        public ClassGroupRepository(DataContext context) : base(context) { }
    }
}
