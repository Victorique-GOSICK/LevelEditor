//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <D3DX11tex.h>
#include "../Core/Utils.h"

#include "../Renderer/Texture.h"
#include "../Renderer/RenderUtil.h"
#include "../Core/Logger.h"
#include "../Core/FileUtils.h"

#include "ResourceManager.h"
#include "TextureFactory.h"
#include <DxErr.h>
#include "../DirectX/DXUtil.h"

namespace LvEdEngine
{
  


// -------------------------------------------------------------------------------------------------
TextureFactory::TextureFactory(ID3D11Device* device)
{
    m_device = device;    
}

// -------------------------------------------------------------------------------------------------
Resource* TextureFactory::CreateResource(Resource* def)
{
    return new Texture((Texture*)def);
}

// -------------------------------------------------------------------------------------------------
bool TextureFactory::LoadResource(Resource* resource, const WCHAR * filename)
{

    if(!FileUtils::Exists(filename)) return false;
    HRESULT hr = S_OK;
    DirectX::TexMetadata metadata;
    DirectX::ScratchImage sourceScratch;
    hr = DXUtil::LoadTexture(filename,&metadata,sourceScratch);    
    if (Logger::IsFailureLog(hr, L"LoadTexture"))
    {
        return false;
    }

    ID3D11Texture2D* dxtex = NULL;
    std::wstring ext = FileUtils::GetExtensionLower(filename);
    // generate full mip chains for non dds file.
    if(ext != L".dds")
    {
        const DirectX::Image* srcImage = sourceScratch.GetImage(0, 0, 0);
        DirectX::ScratchImage mipScratch;
        hr = DirectX::GenerateMipMaps(*srcImage, DirectX::TEX_FILTER_LINEAR, 0, mipScratch, false);
        if (Logger::IsFailureLog(hr, L"DirectX::GenerateMipMaps"))
            return false;    

        hr = DirectX::CreateTexture(m_device, mipScratch.GetImages(), mipScratch.GetImageCount(), mipScratch.GetMetadata(),
                                    (ID3D11Resource**)&dxtex);
       

    }
    else
    {
        hr = DirectX::CreateTexture(m_device, sourceScratch.GetImages(), sourceScratch.GetImageCount(), metadata,(ID3D11Resource**)&dxtex);
    }

    if (Logger::IsFailureLog(hr, L"DirectX::CreateTexture"))
            return false;    
    
    ID3D11ShaderResourceView* texview = CreateTextureView(m_device, dxtex);
    if(!dxtex) return false;

    Texture* tex = (Texture*)resource;    
    tex->Set(dxtex,texview);    
    return true;
}


}; // namespace 




