using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sio.Cms.Lib.Models.Cms;
using Sio.Cms.Lib.Services;
using Sio.Cms.Lib.ViewModels.SioSystem;
using Sio.Common.Helper;
using Sio.Domain.Core.Models;
using Sio.Domain.Core.ViewModels;
using Sio.Domain.Data.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Sio.Cms.Lib.SioEnums;

namespace Sio.Cms.Lib.ViewModels.SioArticles
{
    public class SyncViewModel
         : ViewModelBase<SioCmsContext, SioArticle, SyncViewModel>
    {

        #region Properties

        #region Models

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonIgnore]
        [JsonProperty("extraFields")]
        public string ExtraFields { get; set; } = "[]";

        [JsonIgnore]
        [JsonProperty("extraProperties")]
        public string ExtraProperties { get; set; } = "[]";

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("seoName")]
        public string SeoName { get; set; }

        [JsonProperty("seoTitle")]
        public string SeoTitle { get; set; }

        [JsonProperty("seoDescription")]
        public string SeoDescription { get; set; }

        [JsonProperty("seoKeywords")]
        public string SeoKeywords { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("views")]
        public int? Views { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("publishedDateTime")]
        public DateTime? PublishedDateTime { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("lastModified")]
        public DateTime? LastModified { get; set; }

        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; } = "[]";

        [JsonProperty("status")]
        public SioEnums.SioContentStatus Status { get; set; }

        #endregion Models

        #region Views

        [JsonProperty("domain")]
        public string Domain => SioService.GetConfig<string>("Domain");

        [JsonProperty("categories")]
        public List<SioPageArticles.ReadViewModel> Pages { get; set; }

        [JsonProperty("modules")]
        public List<SioModuleArticles.ReadViewModel> Modules { get; set; } // Parent to Modules

        [JsonProperty("mediaNavs")]
        public List<SioArticleMedias.ReadViewModel> MediaNavs { get; set; }

        [JsonProperty("moduleNavs")]
        public List<SioArticleModules.ReadViewModel> ModuleNavs { get; set; }

        [JsonProperty("articleNavs")]
        public List<SioArticleArticles.ReadViewModel> ArticleNavs { get; set; }

        [JsonProperty("listTag")]
        public JArray ListTag { get; set; } = new JArray();

        [JsonProperty("imageFileStream")]
        public FileStreamViewModel ImageFileStream { get; set; }

        [JsonProperty("thumbnailFileStream")]
        public FileStreamViewModel ThumbnailFileStream { get; set; }

        #region Template

        [JsonProperty("view")]
        public SioTemplates.UpdateViewModel View { get; set; }

        [JsonProperty("templates")]
        public List<SioTemplates.UpdateViewModel> Templates { get; set; }// Article Templates

        [JsonIgnore]
        public int ActivedTheme
        {
            get
            {
                return SioService.GetConfig<int>(SioConstants.ConfigurationKeyword.ThemeId, Specificulture);
            }
        }

        [JsonIgnore]
        public string TemplateFolderType
        {
            get
            {
                return SioEnums.EnumTemplateFolder.Articles.ToString();
            }
        }

        [JsonProperty("templateFolder")]
        public string TemplateFolder
        {
            get
            {
                return CommonHelper.GetFullPath(new string[]
                {
                    SioConstants.Folder.TemplatesFolder
                    , SioService.GetConfig<string>(SioConstants.ConfigurationKeyword.ThemeName, Specificulture)
                    , TemplateFolderType
                }
            );
            }
        }

        #endregion Template

        [JsonProperty("imageUrl")]
        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(Image) && (Image.IndexOf("http") == -1) && Image[0] != '/')
                {
                    return CommonHelper.GetFullPath(new string[] {
                    Domain,  Image
                });
                }
                else
                {
                    return Image;
                }
            }
        }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl
        {
            get
            {
                if (Thumbnail != null && Thumbnail.IndexOf("http") == -1 && Thumbnail[0] != '/')
                {
                    return CommonHelper.GetFullPath(new string[] {
                    Domain,  Thumbnail
                });
                }
                else
                {
                    return string.IsNullOrEmpty(Thumbnail) ? ImageUrl : Thumbnail;
                }
            }
        }

        [JsonProperty("properties")]
        public List<ExtraProperty> Properties { get; set; }

        [JsonProperty("detailsUrl")]
        public string DetailsUrl { get; set; }

        [JsonProperty("urlAliases")]
        public List<SioUrlAliases.UpdateViewModel> UrlAliases { get; set; }

        [JsonProperty("columns")]
        public List<ModuleFieldViewModel> Columns { get; set; }
        #endregion Views

        #endregion Properties

        #region Contructors

        public SyncViewModel() : base()
        {
        }

        public SyncViewModel(SioArticle model, SioCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion Contructors

        #region Overrides

        public override void ExpandView(SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (Id == 0)
            {
                ExtraFields = SioService.GetConfig<string>("DefaultArticleAttr");
            }
            Cultures = LoadCultures(Specificulture, _context, _transaction);
            UrlAliases = GetAliases(_context, _transaction);
            if (!string.IsNullOrEmpty(this.Tags))
            {
                ListTag = JArray.Parse(this.Tags);
            }

            // Parsing Extra Properties fields
            Columns = new List<ModuleFieldViewModel>();
            JArray arrField = !string.IsNullOrEmpty(ExtraFields) ? JArray.Parse(ExtraFields) : new JArray();
            foreach (var field in arrField)
            {
                ModuleFieldViewModel thisField = new ModuleFieldViewModel()
                {
                    Name = CommonHelper.ParseJsonPropertyName(field["name"].ToString()),
                    Title = field["title"]?.ToString(),
                    Options = field["options"] != null ? field["options"].Value<JArray>() : new JArray(),
                    Priority = field["priority"] != null ? field["priority"].Value<int>() : 0,
                    DataType = (SioDataType)(int)field["dataType"],
                    Width = field["width"] != null ? field["width"].Value<int>() : 3,
                    IsUnique = field["isUnique"] != null ? field["isUnique"].Value<bool>() : true,
                    IsRequired = field["isRequired"] != null ? field["isRequired"].Value<bool>() : true,
                    IsDisplay = field["isDisplay"] != null ? field["isDisplay"].Value<bool>() : true,
                    IsSelect = field["isSelect"] != null ? field["isSelect"].Value<bool>() : false,
                    IsGroupBy = field["isGroupBy"] != null ? field["isGroupBy"].Value<bool>() : false,
                };
                Columns.Add(thisField);
            }

            // Parsing Extra Properties value
            Properties = new List<ExtraProperty>();

            if (!string.IsNullOrEmpty(ExtraProperties))
            {
                JArray arr = JArray.Parse(ExtraProperties);
                foreach (JToken item in arr)
                {
                    Properties.Add(item.ToObject<ExtraProperty>());
                }
            }
            //Get Templates
            this.Templates = this.Templates ?? SioTemplates.UpdateViewModel.Repository.GetModelListBy(
                t => t.Theme.Id == ActivedTheme && t.FolderType == this.TemplateFolderType).Data;
            View = SioTemplates.UpdateViewModel.GetTemplateByPath(Template, Specificulture, SioEnums.EnumTemplateFolder.Articles, _context, _transaction);

            this.Template = CommonHelper.GetFullPath(new string[]
               {
                    this.View?.FileFolder
                    , this.View?.FileName
               });

            var getPageArticle = SioPageArticles.ReadViewModel.GetPageArticleNavAsync(Id, Specificulture, _context, _transaction);
            if (getPageArticle.IsSucceed)
            {
                this.Pages = getPageArticle.Data;
                this.Pages.ForEach(c =>
                {
                    c.IsActived = SioPageArticles.ReadViewModel.Repository.CheckIsExists(n => n.CategoryId == c.CategoryId && n.ArticleId == Id, _context, _transaction);
                });
            }

            var getModuleArticle = SioModuleArticles.ReadViewModel.GetModuleArticleNavAsync(Id, Specificulture, _context, _transaction);
            if (getModuleArticle.IsSucceed)
            {
                this.Modules = getModuleArticle.Data;
                this.Modules.ForEach(c =>
                {
                    c.IsActived = SioModuleArticles.ReadViewModel.Repository.CheckIsExists(n => n.ModuleId == c.ModuleId && n.ArticleId == Id, _context, _transaction);
                });
            }
            var otherModules = SioModules.ReadListItemViewModel.Repository.GetModelListBy(
                m => (m.Type == (int)SioEnums.SioModuleType.Content || m.Type == (int)SioEnums.SioModuleType.ListArticle)
                && m.Specificulture == Specificulture
                && !Modules.Any(n => n.ModuleId == m.Id && n.Specificulture == m.Specificulture)
                , "CreatedDateTime", 1, null, 0, _context, _transaction);
            foreach (var item in otherModules.Data.Items)
            {
                Modules.Add(new SioModuleArticles.ReadViewModel()
                {
                    ModuleId = item.Id,
                    Image = item.Image,
                    ArticleId = Id,
                    Description = Title
                });
            }

            // Medias
            var getArticleMedia = SioArticleMedias.ReadViewModel.Repository.GetModelListBy(n => n.ArticleId == Id && n.Specificulture == Specificulture, _context, _transaction);
            if (getArticleMedia.IsSucceed)
            {
                MediaNavs = getArticleMedia.Data.OrderBy(p => p.Priority).ToList();
                MediaNavs.ForEach(n => n.IsActived = true);
            }
            // Modules
            var getArticleModule = SioArticleModules.ReadViewModel.Repository.GetModelListBy(
                n => n.ArticleId == Id && n.Specificulture == Specificulture, _context, _transaction);
            if (getArticleModule.IsSucceed)
            {
                ModuleNavs = getArticleModule.Data.OrderBy(p => p.Priority).ToList();
                foreach (var item in ModuleNavs)
                {
                    item.IsActived = true;
                    item.Module.LoadData(articleId: Id, _context: _context, _transaction: _transaction);
                }
            }
            var otherModuleNavs = SioModules.ReadMvcViewModel.Repository.GetModelListBy(
                m => (m.Type == (int)SioEnums.SioModuleType.SubArticle) && m.Specificulture == Specificulture
                && !ModuleNavs.Any(n => n.ModuleId == m.Id), "CreatedDateTime", 1, null, 0, _context, _transaction);
            foreach (var item in otherModuleNavs.Data.Items)
            {
                item.LoadData(articleId: Id, _context: _context, _transaction: _transaction);
                ModuleNavs.Add(new SioArticleModules.ReadViewModel()
                {
                    ModuleId = item.Id,
                    Image = item.Image,
                    ArticleId = Id,
                    Description = item.Title,
                    Module = item
                });
            }

            // Related Articles
            ArticleNavs = GetRelated(_context, _transaction);
            var otherArticles = SioArticles.ReadListItemViewModel.Repository.GetModelListBy(
                m => m.Id != Id && m.Specificulture == Specificulture
                    && !ArticleNavs.Any(n => n.SourceId == Id)
                    , "CreatedDateTime", 1, 10, 0, _context, _transaction);
            foreach (var item in otherArticles.Data.Items)
            {
                ArticleNavs.Add(new SioArticleArticles.ReadViewModel()
                {
                    SourceId = Id,
                    Image = item.ImageUrl,
                    DestinationId = item.Id,
                    Description = item.Title
                });
            }
        }

        public override SioArticle ParseModel(SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (Id == 0)
            {
                Id = Repository.Max(c => c.Id, _context, _transaction).Data + 1;
                CreatedDateTime = DateTime.UtcNow;
            }
            LastModified = DateTime.UtcNow;
            PublishedDateTime = PublishedDateTime?.ToUniversalTime();

            //  Parsing Extra Fields to json string
            var arrField = Columns != null ? JArray.Parse(
                Newtonsoft.Json.JsonConvert.SerializeObject(Columns.OrderBy(c => c.Priority).Where(
                    c => !string.IsNullOrEmpty(c.Name)))) : new JArray();
            ExtraFields = arrField.ToString(Newtonsoft.Json.Formatting.None);

            // Parsing Extra Properties value
            if (Properties != null && Properties.Count > 0)
            {
                JArray arrProperties = new JArray();
                foreach (var p in Properties.Where(p => !string.IsNullOrEmpty(p.Value) && !string.IsNullOrEmpty(p.Name)))
                {
                    arrProperties.Add(JObject.FromObject(p));
                }
                ExtraProperties = arrProperties.ToString(Formatting.None)?.Trim();
            }

            Template = View != null ? string.Format(@"{0}/{1}{2}", View.FolderType, View.FileName, View.Extension) : Template;
            if (ThumbnailFileStream != null)
            {
                string folder = CommonHelper.GetFullPath(new string[]
                {
                    SioConstants.Folder.UploadFolder, "Articles", DateTime.UtcNow.ToString("dd-MM-yyyy")
                });
                string filename = CommonHelper.GetRandomName(ThumbnailFileStream.Name);
                bool saveThumbnail = CommonHelper.SaveFileBase64(folder, filename, ThumbnailFileStream.Base64);
                if (saveThumbnail)
                {
                    CommonHelper.RemoveFile(Thumbnail);
                    Thumbnail = CommonHelper.GetFullPath(new string[] { folder, filename });
                }
            }
            if (ImageFileStream != null)
            {
                string folder = CommonHelper.GetFullPath(new string[]
                {
                    SioConstants.Folder.UploadFolder, "Articles", DateTime.UtcNow.ToString("dd-MM-yyyy")
                });
                string filename = CommonHelper.GetRandomName(ImageFileStream.Name);
                bool saveImage = CommonHelper.SaveFileBase64(folder, filename, ImageFileStream.Base64);
                if (saveImage)
                {
                    CommonHelper.RemoveFile(Image);
                    Image = CommonHelper.GetFullPath(new string[] { folder, filename });
                }
            }

            if (!string.IsNullOrEmpty(Image) && Image[0] == '/') { Image = Image.Substring(1); }
            if (!string.IsNullOrEmpty(Thumbnail) && Thumbnail[0] == '/') { Thumbnail = Thumbnail.Substring(1); }
            Tags = ListTag.ToString(Newtonsoft.Json.Formatting.None);
            GenerateSEO();

            return base.ParseModel(_context, _transaction);
        }

        #region Async Methods

        public override async Task<RepositoryResponse<bool>> SaveSubModelsAsync(
            SioArticle parent
            , SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = new RepositoryResponse<bool>() { IsSucceed = true };
            try
            {               
                if (result.IsSucceed)
                {
                    foreach (var item in UrlAliases)
                    {
                        item.SourceId = parent.Id.ToString();
                        item.Type = SioEnums.UrlAliasType.Article;
                        item.Specificulture = Specificulture;
                        var saveResult = await item.SaveModelAsync(false, _context, _transaction);
                        result.IsSucceed = saveResult.IsSucceed;
                        if (!result.IsSucceed)
                        {
                            result.Exception = saveResult.Exception;
                            result.Errors.AddRange(saveResult.Errors);
                            break;
                        }
                    }
                }
                if (result.IsSucceed)
                {
                    var startMediaId = SioMedias.UpdateViewModel.Repository.Max(c => c.Id,_context, _transaction).Data;
                    foreach (var navMedia in MediaNavs)
                    {                        
                        if (navMedia.Media != null)
                        {
                            startMediaId += 1;
                            navMedia.Media.Specificulture = Specificulture;
                            navMedia.Media.Id = startMediaId;
                            navMedia.Media.Cultures = Cultures;
                            var saveMedia = await navMedia.Media.SaveModelAsync(false, _context, _transaction);
                            if (saveMedia.IsSucceed)
                            {
                                navMedia.ArticleId = parent.Id;
                                navMedia.MediaId = saveMedia.Data.Model.Id;
                                navMedia.Specificulture = parent.Specificulture;
                                var saveResult = await navMedia.SaveModelAsync(false, _context, _transaction);
                                result.IsSucceed = saveResult.IsSucceed;
                                if (!result.IsSucceed)
                                {
                                    result.Exception = saveResult.Exception;
                                    Errors.AddRange(saveResult.Errors);
                                }
                            }
                            else
                            {
                                result.IsSucceed = false;
                                result.Exception = saveMedia.Exception;
                                Errors.AddRange(saveMedia.Errors);
                            }
                        }
                    }
                }
                if (result.IsSucceed)
                {
                    foreach (var navModule in ModuleNavs)
                    {
                        navModule.ArticleId = parent.Id;
                        navModule.Specificulture = parent.Specificulture;
                        navModule.Status = SioEnums.SioContentStatus.Published;
                        if (navModule.IsActived)
                        {
                            var saveResult = await navModule.SaveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = await navModule.RemoveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                if (result.IsSucceed)
                {
                    foreach (var navArticle in ArticleNavs)
                    {
                        navArticle.SourceId = parent.Id;
                        navArticle.Status = SioEnums.SioContentStatus.Published;
                        navArticle.Specificulture = parent.Specificulture;
                        if (navArticle.IsActived)
                        {
                            var saveResult = await navArticle.SaveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = await navArticle.RemoveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }
                if (result.IsSucceed)
                {
                    // Save Parent Category
                    foreach (var item in Pages)
                    {
                        item.ArticleId = parent.Id;
                        item.Description = parent.Title;
                        item.Image = ThumbnailUrl;
                        item.Status = SioEnums.SioContentStatus.Published;
                        if (item.IsActived)
                        {
                            var saveResult = await item.SaveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = await item.RemoveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                if (result.IsSucceed)
                {
                    // Save Parent Modules
                    foreach (var item in Modules)
                    {
                        item.ArticleId = parent.Id;
                        item.Description = parent.Title;
                        item.Image = ThumbnailUrl;
                        item.Status = SioEnums.SioContentStatus.Published;
                        if (item.IsActived)
                        {
                            var saveResult = await item.SaveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = await item.RemoveModelAsync(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex) // TODO: Add more specific exeption types instead of Exception only
            {
                result.IsSucceed = false;
                result.Exception = ex;
                return result;
            }
        }

        #endregion Async Methods

        #region Sync Methods

        public override RepositoryResponse<bool> SaveSubModels(
            SioArticle parent
            , SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = new RepositoryResponse<bool>() { IsSucceed = true };
            try
            {
                
                if (result.IsSucceed)
                {
                    var startMediaId = SioMedias.UpdateViewModel.Repository.Max(c => c.Id).Data;
                    foreach (var navMedia in MediaNavs)
                    {
                        if (navMedia.Media != null)
                        {
                            startMediaId += 1;
                            navMedia.Media.Specificulture = Specificulture;
                            navMedia.Media.Cultures = Cultures;
                            navMedia.Media.Id = startMediaId;
                            var saveMedia =  navMedia.Media.SaveModel(false, _context, _transaction);
                            if (saveMedia.IsSucceed)
                            {
                                navMedia.ArticleId = parent.Id;
                                navMedia.MediaId = saveMedia.Data.Model.Id;
                                navMedia.Specificulture = parent.Specificulture;
                                var saveResult = navMedia.SaveModel(false, _context, _transaction);
                                result.IsSucceed = saveResult.IsSucceed;
                                if (!result.IsSucceed)
                                {
                                    result.Exception = saveResult.Exception;
                                    Errors.AddRange(saveResult.Errors);
                                }
                            }
                            else
                            {
                                result.IsSucceed = false;
                                result.Exception = saveMedia.Exception;
                                Errors.AddRange(saveMedia.Errors);
                            }
                        }
                    }
                }
                if (result.IsSucceed)
                {
                    foreach (var navModule in ModuleNavs)
                    {
                        navModule.ArticleId = parent.Id;
                        navModule.Specificulture = parent.Specificulture;
                        navModule.Status = SioEnums.SioContentStatus.Published;
                        if (navModule.IsActived)
                        {
                            var saveResult = navModule.SaveModel(true, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = navModule.RemoveModel(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                
                if (result.IsSucceed)
                {
                    // Save Parent Category
                    foreach (var item in Pages)
                    {
                        item.ArticleId = parent.Id;
                        item.Description = parent.Title;
                        item.Image = ThumbnailUrl;
                        item.Status = SioEnums.SioContentStatus.Published;
                        if (item.IsActived)
                        {
                            var saveResult = item.SaveModel(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = item.RemoveModel(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                if (result.IsSucceed)
                {
                    // Save Parent Modules
                    foreach (var item in Modules)
                    {
                        item.ArticleId = parent.Id;
                        item.Description = parent.Title;
                        item.Image = ThumbnailUrl;
                        item.Status = SioEnums.SioContentStatus.Published;
                        if (item.IsActived)
                        {
                            var saveResult = item.SaveModel(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                        else
                        {
                            var saveResult = item.RemoveModel(false, _context, _transaction);
                            result.IsSucceed = saveResult.IsSucceed;
                            if (!result.IsSucceed)
                            {
                                result.Exception = saveResult.Exception;
                                Errors.AddRange(saveResult.Errors);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex) // TODO: Add more specific exeption types instead of Exception only
            {
                result.IsSucceed = false;
                result.Exception = ex;
                return result;
            }
        }

        public override RepositoryResponse<bool> RemoveRelatedModels(SyncViewModel view, SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            RepositoryResponse<bool> result = new RepositoryResponse<bool>()
            {
                IsSucceed = true
            };

            if (result.IsSucceed)
            {
                var navCate = _context.SioPageArticle.Where(n => n.ArticleId == Id && n.Specificulture == Specificulture).ToList();
                foreach (var item in navCate)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            if (result.IsSucceed)
            {
                var navModule = _context.SioModuleArticle.Where(n => n.ArticleId == Id && n.Specificulture == Specificulture).ToList();
                foreach (var item in navModule)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            if (result.IsSucceed)
            {
                var navMedia = _context.SioArticleMedia.Where(n => n.ArticleId == Id && n.Specificulture == Specificulture).ToList();
                foreach (var item in navMedia)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }
            if (result.IsSucceed)
            {
                var navModule = _context.SioArticleModule.Where(n => n.ArticleId == Id && n.Specificulture == Specificulture).ToList();
                foreach (var item in navModule)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            if (result.IsSucceed)
            {
                var navRelated = _context.SioArticleMedia.Where(n => n.ArticleId == Id && n.Specificulture == Specificulture).ToList();
                foreach (var item in navRelated)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            if (result.IsSucceed)
            {
                var navs = _context.SioUrlAlias.Where(n => n.SourceId == Id.ToString() && n.Type == (int)SioEnums.UrlAliasType.Article && n.Specificulture == Specificulture).ToList();
                foreach (var item in navs)
                {
                    _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            result.IsSucceed = (_context.SaveChanges() > 0);
            return result;
        }

        #endregion  Methods

        #endregion Overrides

        #region Expands
        List<SupportedCulture> LoadCultures(string initCulture = null, SioCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getCultures = SystemCultureViewModel.Repository.GetModelList(_context, _transaction);
            var result = new List<SupportedCulture>();
            if (getCultures.IsSucceed)
            {
                foreach (var culture in getCultures.Data)
                {
                    result.Add(
                        new SupportedCulture()
                        {
                            Icon = culture.Icon,
                            Specificulture = culture.Specificulture,
                            Alias = culture.Alias,
                            FullName = culture.FullName,
                            Description = culture.FullName,
                            Id = culture.Id,
                            Lcid = culture.Lcid,
                            IsSupported = culture.Specificulture == initCulture || _context.SioArticle.Any(p => p.Id == Id && p.Specificulture == culture.Specificulture)
                        });

                }
            }
            return result;
        }

        private void GenerateSEO()
        {
            if (string.IsNullOrEmpty(this.SeoName))
            {
                this.SeoName = SeoHelper.GetSEOString(this.Title);
            }
            int i = 1;
            string name = SeoName;
            while (UpdateViewModel.Repository.CheckIsExists(a => a.SeoName == name && a.Specificulture == Specificulture && a.Id != Id))
            {
                name = SeoName + "_" + i;
                i++;
            }
            SeoName = name;

            if (string.IsNullOrEmpty(this.SeoTitle))
            {
                this.SeoTitle = SeoHelper.GetSEOString(this.Title);
            }

            if (string.IsNullOrEmpty(this.SeoDescription))
            {
                this.SeoDescription = SeoHelper.GetSEOString(this.Title);
            }

            if (string.IsNullOrEmpty(this.SeoKeywords))
            {
                this.SeoKeywords = SeoHelper.GetSEOString(this.Title);
            }
        }

        public List<SioArticleArticles.ReadViewModel> GetRelated(SioCmsContext context, IDbContextTransaction transaction)
        {
            var navs = SioArticleArticles.ReadViewModel.Repository.GetModelListBy(n => n.SourceId == Id && n.Specificulture == Specificulture, context, transaction).Data;
            navs.ForEach(n => n.IsActived = true);
            return navs.OrderBy(p => p.Priority).ToList();
        }
        public List<SioUrlAliases.UpdateViewModel> GetAliases(SioCmsContext context, IDbContextTransaction transaction)
        {
            var result = SioUrlAliases.UpdateViewModel.Repository.GetModelListBy(p => p.Specificulture == Specificulture
                        && p.SourceId == Id.ToString() && p.Type == (int)SioEnums.UrlAliasType.Article, context, transaction);
            if (result.IsSucceed)
            {
                return result.Data;
            }
            else
            {
                return new List<SioUrlAliases.UpdateViewModel>();
            }
        }
        #endregion Expands
    }
}
