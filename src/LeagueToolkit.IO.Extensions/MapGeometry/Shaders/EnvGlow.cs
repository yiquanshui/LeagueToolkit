﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using LeagueToolkit.Core.Environment;
using LeagueToolkit.Core.Renderer;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.Meta.Classes;
using SharpGLTF.Schema2;
using GltfImage = SharpGLTF.Schema2.Image;
using TextureRegistry = System.Collections.Generic.Dictionary<string, SharpGLTF.Schema2.Image>;

namespace LeagueToolkit.IO.Extensions.MapGeometry.Shaders;

internal sealed class EnvGlow : IMaterialAdapter
{
    public void InitializeMaterial(
        Material gltfMaterial,
        StaticMaterialDef materialDef,
        EnvironmentAssetMesh mesh,
        TextureRegistry textureRegistry,
        ModelRoot root,
        MapGeometryGltfConversionContext context
    )
    {
        gltfMaterial.InitializeUnlit();

        InitializeMaterialRenderTechnique(gltfMaterial, materialDef);
        InitializeMaterialBaseColorChannel(gltfMaterial, materialDef, mesh, root, textureRegistry, context);
    }

    private static void InitializeMaterialRenderTechnique(Material gltfMaterial, StaticMaterialDef materialDef)
    {
        StaticMaterialTechniqueDef techniqueDef = materialDef.Techniques.FirstOrDefault() ?? new(new());
        StaticMaterialPassDef passDef = techniqueDef.Passes.FirstOrDefault() ?? new(new());

        if (passDef.BlendEnable)
            gltfMaterial.Alpha = AlphaMode.BLEND;
    }

    private static void InitializeMaterialBaseColorChannel(
        Material gltfMaterial,
        StaticMaterialDef materialDef,
        EnvironmentAssetMesh mesh,
        ModelRoot root,
        TextureRegistry textureRegistry,
        MapGeometryGltfConversionContext context
    )
    {
        StaticMaterialShaderSamplerDef samplerDef = materialDef.SamplerValues.FirstOrDefault(x =>
            x.Value.TextureName is "GlowTexture"
        );

        StaticMaterialShaderParamDef colorMultDef = materialDef.ParamValues.FirstOrDefault(x =>
            x.Value.Name is "Color_Mult"
        );
        StaticMaterialShaderParamDef alphaMultDef = materialDef.ParamValues.FirstOrDefault(x =>
            x.Value.Name is "Alpha_Mult"
        );

        gltfMaterial.WithChannelColor(
            "BaseColor",
            (colorMultDef?.Value ?? Vector4.One) with
            {
                W = alphaMultDef?.Value.X ?? 1f
            }
        );
        gltfMaterial.WithChannelTexture(
            "BaseColor",
            0,
            TextureUtils.CreateGltfImage(samplerDef.TexturePath, root, textureRegistry, context)
        );
    }
}
