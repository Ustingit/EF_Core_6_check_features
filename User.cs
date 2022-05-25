using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		public int? CompanyId { get; set; }
		public Company? Company { get; set; }

		[Required]
		public int PositionId { get; set; }
		public Position Position { get; set; }
    }
}
