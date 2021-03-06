using Microsoft.EntityFrameworkCore.Storage;
using Sio.Cms.Lib.Models.Cms;
using Sio.Domain.Data.ViewModels;
using Newtonsoft.Json;
using System;

namespace Sio.Cms.Lib.ViewModels.SioProductProducts
{
    public class ReadViewModel
        : ViewModelBase<SioCmsContext, SioRelatedProduct, ReadViewModel>
    {
        #region Properties

        #region Models

        [JsonProperty("sourceId")]
        public int SourceId { get; set; }

        [JsonProperty("destinationId")]
        public int DestinationId { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        #endregion Models

        #region Views

        [JsonProperty("isActived")]
        public bool IsActived { get; set; }

        [JsonProperty("relatedProduct")]
        public SioProducts.ReadListItemViewModel RelatedProduct { get; set; }

        #endregion Views

        #endregion Properties

        #region Contructors

        public ReadViewModel() : base()
        {
        }

        public ReadViewModel(SioRelatedProduct model, SioCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion Contructors

        #region Overrides

        public override void ExpandView(SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getProduct = SioProducts.ReadListItemViewModel.Repository.GetSingleModel(
                m => m.Id == DestinationId && m.Specificulture == Specificulture
                , _context: _context, _transaction: _transaction);
            if (getProduct.IsSucceed)
            {
                this.RelatedProduct = getProduct.Data;
            }
        }

        public override SioRelatedProduct ParseModel(SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (CreatedDateTime == default(DateTime))
            {
                CreatedDateTime = DateTime.UtcNow;
            }
            return base.ParseModel(_context, _transaction);
        }

        #endregion Overrides
    }
}
