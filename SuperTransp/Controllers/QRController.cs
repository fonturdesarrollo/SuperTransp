using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SuperTransp.Core;
using System.Text;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Controllers
{
	public class QRController : Controller
	{
		private IPublicTransportGroup _publicTransportGroup;
		private IDriver _driver;
		ISupervision _supervision;
		ISecurity _security;
		private readonly QrCodeSigner _qrCodeSigner;

		public QRController(IPublicTransportGroup publicTransportGroup, IDriver driver, ISupervision supervision, ISecurity security, QrCodeSigner qrCodeSigner)
		{
			_qrCodeSigner = qrCodeSigner;
			_publicTransportGroup = publicTransportGroup;
			_driver = driver;
			_supervision = supervision;
			_security = security;
		}

		public IActionResult PublicTransportGroupData(string ptgCode)
		{
			var model = _publicTransportGroup.GetByGUIDId(ptgCode);

			ViewBag.AllowedHash = _security.GeneratePublicKey();

			return View(model);
		}

		public IActionResult DriverData(string driverCode)
		{
			// Malformed or forged codes (bad signature) are answered like a code that does not exist
			if (!_qrCodeSigner.TryParseDriverCode(driverCode, out var publicTransportGroupGUID, out var partnerNumber))
			{
				return NotFound();
			}

			var model = _supervision.GetByPublicTransportGroupGUIDAndPartnerNumber(publicTransportGroupGUID, partnerNumber);

			if (model == null || model.PublicTransportGroupId == 0)
			{
				return NotFound();
			}

			ViewBag.AllowedHash = _security.GeneratePublicKey();

			return View(model);
		}
	}
}
