// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modouv.Fractales.World.Objects
{
    /// <summary>
    /// Emcapsule plusieurs objets Model de qualité différentes. 
    /// </summary>
    public class ModelMipmap 
    {
        public enum ModelQuality { High, Medium, Low }
        public ModelData this[ModelQuality quality]
        {
            get
            {
                switch(quality)
                {
                    case ModelQuality.Low:
                        return LowQualityModel;
                    case ModelQuality.Medium:
                        return MediumQualityModel;
                    case ModelQuality.High:
                        return HighQualityModel;
                    default:
                        return HighQualityModel;
                }
            }
            set
            {
                switch (quality)
                {
                    case ModelQuality.Low:
                        LowQualityModel = value;
                        break;
                    case ModelQuality.Medium:
                        MediumQualityModel = value;
                        break;
                    case ModelQuality.High:
                        HighQualityModel = value;
                        break;
                    default:
                        HighQualityModel = value;
                        break;
                }
            }
        }
        public ModelData HighQualityModel { get; set;}
        public ModelData LowQualityModel { get; set;}
        public ModelData MediumQualityModel { get; set; }
        public void Dispose()
        {
            HighQualityModel.Vertices.Dispose();
            HighQualityModel.Indices.Dispose();
            LowQualityModel.Vertices.Dispose();
            LowQualityModel.Indices.Dispose();
            MediumQualityModel.Vertices.Dispose();
            MediumQualityModel.Indices.Dispose();
        }
    }
}
