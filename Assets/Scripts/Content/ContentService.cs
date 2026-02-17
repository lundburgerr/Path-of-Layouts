using fireMCG.PathOfLayouts.Campaign;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace fireMCG.PathOfLayouts.Content
{
    public sealed class ContentService
    {
        private readonly CampaignDatabase _database;

        private readonly Dictionary<string, AsyncOperationHandle<Texture2D>> _textureHandlesByGuid =
            new Dictionary<string, AsyncOperationHandle<Texture2D>>(StringComparer.Ordinal);

        public ContentService(CampaignDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            _database = database;

            return;
        }

        public async Task<long> GetDownloadSizeForAreaGraphRendersAsync(string areaId)
        {
            string label = AddressablesLabels.AREA_GRAPH_RENDERS_PREFIX + areaId;

            long bytes = await GetDownloadSizeForLabelAsync(label);

            return bytes;
        }

        public async Task<long> GetDownloadSizeForGraphLayoutImagesAsync(string graphId)
        {
            string label = AddressablesLabels.GRAPH_LAYOUT_IMAGES_PREFIX + graphId;

            long bytes = await GetDownloadSizeForLabelAsync(label);

            return bytes;
        }

        public async Task PreDownloadAreaGraphRendersAsync(string areaId, CancellationToken token)
        {
            string label = AddressablesLabels.AREA_GRAPH_RENDERS_PREFIX + areaId;

            await PreDownloadLabelAsync(label, token);

            return;
        }

        public async Task PreDownloadGraphLayoutImagesAsync(string graphId, CancellationToken token)
        {
            string label = AddressablesLabels.GRAPH_LAYOUT_IMAGES_PREFIX + graphId;

            await PreDownloadLabelAsync(label, token);

            return;
        }

        public async Task<Texture2D> LoadGraphRenderAsync(string graphId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            GraphDef graph = _database.GetGraph(graphId);
            if (graph == null)
            {
                throw new InvalidOperationException($"ContentService.LoadGraphRenderAsync error, graph not found. graphId={graphId}");
            }

            Texture2D texture = await LoadAddressablesTextureAsync(graph.render, token);

            return texture;
        }

        public async Task<Texture2D> LoadLayoutImageAsync(string layoutId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            LayoutDef layout = _database.GetLayout(layoutId);
            if (layout == null)
            {
                throw new InvalidOperationException($"ContentService.LoadLayoutImageAsync error, layout not found. layoutId={layoutId}");
            }

            Texture2D texture = await LoadAddressablesTextureAsync(layout.layoutImage, token);

            return texture;
        }

        public void ReleaseAll()
        {
            ReleaseAllAddressablesTextures();

            return;
        }

        public void ReleaseLayoutImage(string layoutId)
        {
            if (string.IsNullOrWhiteSpace(layoutId))
            {
                return;
            }

            LayoutDef layout = _database.GetLayout(layoutId);
            if (layout == null)
            {
                return;
            }

            ReleaseAddressablesTexture(layout.layoutImage);

            return;
        }

        public void ReleaseGraphRender(string graphId)
        {
            if (string.IsNullOrWhiteSpace(graphId))
            {
                return;
            }

            GraphDef graph = _database.GetGraph(graphId);
            if (graph == null)
            {
                return;
            }

            ReleaseAddressablesTexture(graph.render);

            return;
        }

        private static async Task<long> GetDownloadSizeForLabelAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return 0;
            }

            AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(label);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);

                return 0;
            }

            long bytes = handle.Result;

            Addressables.Release(handle);

            return bytes;
        }

        private static async Task PreDownloadLabelAsync(string label, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(label, false);
            await handle.Task;

            token.ThrowIfCancellationRequested();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);

                throw new InvalidOperationException($"ContentService.PreDownloadLabelAsync failed. label={label}");
            }

            Addressables.Release(handle);

            return;
        }

        private async Task<Texture2D> LoadAddressablesTextureAsync(AssetReferenceT<Texture2D> reference, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (reference == null)
            {
                throw new InvalidOperationException("ContentService.LoadAddressablesTextureAsync error, reference is null.");
            }

            string guid = reference.AssetGUID;
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new InvalidOperationException("ContentService.LoadAddressablesTextureAsync error, reference.AssetGUID is empty.");
            }

            if (_textureHandlesByGuid.TryGetValue(guid, out AsyncOperationHandle<Texture2D> existingHandle))
            {
                if (existingHandle.IsValid() &&
                    existingHandle.Status == AsyncOperationStatus.Succeeded &&
                    existingHandle.Result != null)
                {
                    return existingHandle.Result;
                }

                _textureHandlesByGuid.Remove(guid);

                if (existingHandle.IsValid())
                {
                    Addressables.Release(existingHandle);
                }
            }

            AsyncOperationHandle<Texture2D> handle = reference.LoadAssetAsync();
            _textureHandlesByGuid[guid] = handle;

            await handle.Task;

            token.ThrowIfCancellationRequested();

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                _textureHandlesByGuid.Remove(guid);

                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw new InvalidOperationException($"ContentService.LoadAddressablesTextureAsync failed. guid={guid}");
            }

            return handle.Result;
        }

        private void ReleaseAddressablesTexture(AssetReferenceT<Texture2D> reference)
        {
            if (reference == null)
            {
                return;
            }

            string guid = reference.AssetGUID;
            if (string.IsNullOrWhiteSpace(guid))
            {
                return;
            }

            if (_textureHandlesByGuid.TryGetValue(guid, out AsyncOperationHandle<Texture2D> handle))
            {
                _textureHandlesByGuid.Remove(guid);

                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            return;
        }

        private void ReleaseAllAddressablesTextures()
        {
            List<string> keys = new List<string>(_textureHandlesByGuid.Keys);

            foreach (string guid in keys)
            {
                AsyncOperationHandle<Texture2D> handle = _textureHandlesByGuid[guid];

                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _textureHandlesByGuid.Clear();

            return;
        }
    }
}