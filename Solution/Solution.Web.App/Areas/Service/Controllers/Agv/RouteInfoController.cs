using Microsoft.AspNet.Identity;
using OSharp.Core.Data;
using OSharp.Core.Data.Extensions;
using OSharp.Utility.Data;
using Solution.Agv.Contracts;
using Solution.Agv.Models;
using Solution.Core.Models.Identity;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Solution.Web.App.Areas.Service.Controllers
{
    [Description("服务-路径管理")]
    public class RouteInfoController : ServiceBaseApiController
    {
        /// <summary>
        /// 路径信息契约
        /// </summary>
        public IRouteInfoContract RouteInfoContract { get; set; }

        /// <summary>
        /// PageRepuestParams举例：
        /// {
        ///     "filterGroup":"",
        ///     "pageIndex":1,
        ///     "pageSize":5,
        ///     "sortField":"Id,NodeId,NodeName,NodeUrl,Interval,Description,IsLocked,CreatedTime",
        ///     "sortOrder":",asc,,,,,,"
        ///}
        /// </summary>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        [Description("服务-路径管理-分页数据")]
        public IHttpActionResult PageData(PageRepuestParams requestParams)
        {
            try
            {
                GridRequest request = new GridRequest(requestParams);
                var page = GetPageResult(RouteInfoContract.RouteInfos, m => new
                {
                    m.Id,
                    m.RouteNo,
                    m.RouteName,
                    m.MarkPoints,
                    m.TotalDistance,
                    StartMarkPointInfo_Id = m.StartMarkPointInfo.Id,
                    EndMarkPointInfo_Id = m.EndMarkPointInfo.Id,
                    StartMarkPointInfoName = m.StartMarkPointInfo.MarkPointName,
                    EndMarkPointInfoName = m.EndMarkPointInfo.MarkPointName,
                    m.Remark,
                    m.CreatedTime,
                    m.CreatorUserId,
                    m.LastUpdatedTime,
                    m.LastUpdatorUserId,
                }, request);
                return Json(new OperationResult(OperationResultType.Success, "读取路径信息列表数据成功！", page));

            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "读取路径信息列表数据失败！" + ex.ToString()));
            }
        }

        [Description("服务-路径管理-增加路径信息")]
        public async Task<IHttpActionResult> Add(params Agv.Dtos.RouteInfoInputDto[] dto)
        {
            var result = await RouteInfoContract.Add(dto);
            return Json(result);
        }

        [Description("服务-路径管理-修改路径信息")]
        public async Task<IHttpActionResult> Update(params Agv.Dtos.RouteInfoInputDto[] dto)
        {
            var result = await RouteInfoContract.Update(dto);
            return Json(result);
        }

        [Description("服务-路径管理-物理删除路径信息")]
        public async Task<IHttpActionResult> Remove(params Guid[] ids)
        {
            var result = await RouteInfoContract.Delete(ids);
            return Json(result);
        }
    }
}
