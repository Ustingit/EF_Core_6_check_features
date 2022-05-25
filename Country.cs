using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	public class Country
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public int CapitalId { get; set; }
		public City? Capital { get; set; }  // столица страны

		public List<Company> Companies { get; set; } = new List<Company>();
	}
}
