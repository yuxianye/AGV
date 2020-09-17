using OSharp.Core.Data;
using OSharp.Utility;
using OSharp.Utility.Data;
using OSharp.Utility.Exceptions;
using OSharp.Utility.Extensions;
using Solution.Agv.Contracts;
using Solution.Agv.Dtos;
using Solution.Agv.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Solution.Agv.Services
{
    /// <summary>
    /// 路径服务
    /// </summary>
    public class RouteInfoService : IRouteInfoContract
    {
        /// <summary>
        /// 路段仓储
        /// </summary>
        public IRepository<RouteInfo, Guid> RouteInfoRepository { get; set; }

        /// <summary>
        /// 地标点仓储
        /// </summary>
        public IRepository<MarkPointInfo, Guid> MarkPointInfoRepository { get; set; }

        /// <summary>
        /// 查询数据集
        /// </summary>
        public IQueryable<RouteInfo> RouteInfos
        {
            get { return RouteInfoRepository.Entities; }
        }

        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckRouteInfoExists(Expression<Func<RouteInfo, bool>> predicate, Guid id)
        {
            return RouteInfoRepository.CheckExists(predicate, id);
        }

        /// <summary>
        /// 增加路径信息
        /// </summary>
        /// <param name="inputDtos"></param>
        /// <returns></returns>
        public async Task<OperationResult> Add(params RouteInfoInputDto[] inputDtos)
        {
            inputDtos.CheckNotNull("inputDtos");
            foreach (var dtoData in inputDtos)
            {
                if (string.IsNullOrEmpty(dtoData.RouteNo))
                    return new OperationResult(OperationResultType.Error, "请正确填写路径编号，该组数据不被存储。");
                if (string.IsNullOrEmpty(dtoData.RouteName))
                    return new OperationResult(OperationResultType.Error, "请正确填写路径名称，该组数据不被存储。");
                if (RouteInfoRepository.CheckExists(x => x.RouteNo == dtoData.RouteNo /*&& x.IsDeleted == false*/))
                    return new OperationResult(OperationResultType.Error, $"路径编号 {dtoData.RouteNo} 的数据已存在，该组数据不被存储。");
                if (RouteInfoRepository.CheckExists(x => x.RouteName == dtoData.RouteName/* && x.IsDeleted == false*/))
                    return new OperationResult(OperationResultType.Error, $"路径名称 {dtoData.RouteName} 的数据已存在，该组数据不被存储。");
                dtoData.StartMarkPointInfo = MarkPointInfoRepository.GetByKey(dtoData.StartMarkPointInfo_Id);
                if (Equals(dtoData.StartMarkPointInfo, null))
                {
                    return new OperationResult(OperationResultType.Error, $"对应的开始地标点不存在,该组数据不被存储。");
                }
                dtoData.EndMarkPointInfo = MarkPointInfoRepository.GetByKey(dtoData.EndMarkPointInfo_Id);
                if (Equals(dtoData.EndMarkPointInfo, null))
                {
                    return new OperationResult(OperationResultType.Error, $"对应的结束地标点不存在,该组数据不被存储。");
                }
            }
            RouteInfoRepository.UnitOfWork.BeginTransaction();
            var result = await RouteInfoRepository.InsertAsync(inputDtos);
            RouteInfoRepository.UnitOfWork.Commit();
            return result;
        }

        /// <summary>
        /// 更新路径信息
        /// </summary>
        /// <param name="inputDtos"></param>
        /// <returns></returns>
        public async Task<OperationResult> Update(params RouteInfoInputDto[] inputDtos)
        {
            inputDtos.CheckNotNull("inputDtos");
            foreach (var dtoData in inputDtos)
            {
                if (string.IsNullOrEmpty(dtoData.RouteNo))
                    return new OperationResult(OperationResultType.Error, "请正确填写路径编号，该组数据不被存储。");
                if (string.IsNullOrEmpty(dtoData.RouteName))
                    return new OperationResult(OperationResultType.Error, "请正确填写路径名称，该组数据不被存储。");
                if (RouteInfoRepository.CheckExists(x => x.RouteNo == dtoData.RouteNo && x.Id != dtoData.Id))
                    return new OperationResult(OperationResultType.Error, $"路径编号 {dtoData.RouteNo} 的数据已存在，该组数据不被存储。");
                if (RouteInfoRepository.CheckExists(x => x.RouteName == dtoData.RouteName && x.Id != dtoData.Id))
                    return new OperationResult(OperationResultType.Error, $"路径名称 {dtoData.RouteName} 的数据已存在，该组数据不被存储。");

                dtoData.StartMarkPointInfo = MarkPointInfoRepository.GetByKey(dtoData.StartMarkPointInfo_Id);
                if (Equals(dtoData.StartMarkPointInfo, null))
                {
                    return new OperationResult(OperationResultType.Error, $"对应的开始地标点不存在,该组数据不被存储。");
                }

                dtoData.EndMarkPointInfo = MarkPointInfoRepository.GetByKey(dtoData.EndMarkPointInfo_Id);

                if (Equals(dtoData.EndMarkPointInfo, null))
                {
                    return new OperationResult(OperationResultType.Error, $"对应的结束地标点不存在,该组数据不被存储。");
                }
            }
            RouteInfoRepository.UnitOfWork.BeginTransaction();
            var result = await RouteInfoRepository.UpdateAsync(inputDtos);
            RouteInfoRepository.UnitOfWork.Commit();
            return result;
        }

        /// <summary>
        /// 物理删除路径信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<OperationResult> Delete(params Guid[] ids)
        {
            ids.CheckNotNull("ids");
            RouteInfoRepository.UnitOfWork.BeginTransaction();
            var result = await RouteInfoRepository.DeleteAsync(ids);
            RouteInfoRepository.UnitOfWork.Commit();
            return result;
        }

    }
}
