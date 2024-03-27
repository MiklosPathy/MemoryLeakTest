using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryLeakTest
{
    public class TestBDContext : DbContext
    {
        public TestBDContext(DbContextOptions<TestBDContext> options)
        : base(options)
        {
        }
        public virtual DbSet<TestEntity> TestEntities { get; set; }
    }
}
