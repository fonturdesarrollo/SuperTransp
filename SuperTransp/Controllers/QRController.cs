using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SuperTransp.Core;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class QRController : Controller
	{
		private IPublicTransportGroup _publicTransportGroup;
		private IDriver _driver;
		ISupervision _supervision;

		public QRController(IPublicTransportGroup publicTransportGroup, IDriver driver, ISupervision supervision)
		{
			_publicTransportGroup = publicTransportGroup;
			_driver = driver;
			_supervision = supervision;
		}

		public IActionResult PublicTransportGroupData(string ptgCode)
		{
			var model = _publicTransportGroup.GetByGUIDId(ptgCode);

			return View(model);
		}

		public IActionResult DriverData(string driverCode)
		{
			string publicTransportGroupGUID = string.Empty;
			int partnerNumber = 0;

			if(!string.IsNullOrEmpty(driverCode))
			{
				var codes = driverCode.Split('|');

				if (codes?.Length > 0)
				{
					publicTransportGroupGUID = codes[0];
					partnerNumber = int.Parse(codes[1]);
				}
			}

			var model = _supervision.GetByPublicTransportGroupGUIDAndPartnerNumber(publicTransportGroupGUID, partnerNumber);

			return View(model);
		}
	}
}
