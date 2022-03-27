#ifndef PLANE_CLIPPING_INCLUDED
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
#define PLANE_CLIPPING_INCLUDED

//Plane clipping definitions

#if CLIP_PLANE || CLIP_TWO_PLANES || CLIP_SPHERE || CLIP_CUBE || CLIP_TUBES || CLIP_SPHERES || FADE_PLANE || FADE_SPHERE|| FADE_SPHERES
	//PLANE_CLIPPING_ENABLED will be defined.
	//PLANE_CLIPPING_ENABLED will be defined.
	//This makes it easier to check if this feature is available or not.
	#define PLANE_CLIPPING_ENABLED 1

#if CLIP_PLANE || CLIP_TWO_PLANES || CLIP_CUBE || CLIP_SPHERE || FADE_PLANE || FADE_SPHERE
	uniform float _SectionOffset = 0;
	uniform float3 _SectionPlane;
	uniform float3 _SectionPoint;

	#if CLIP_TWO_PLANES || CLIP_CUBE
	uniform float3 _SectionPlane2;
	#endif
	#if CLIP_SPHERE || CLIP_CUBE || FADE_SPHERE || FADE_PLANE
	uniform float _Radius = 0;
	#endif

	#if CLIP_CUBE
	static const float3 _SectionPlane3 = normalize(cross(_SectionPlane, _SectionPlane2));
	#endif
#endif

#if CLIP_PLANE || CLIP_CUBE || CLIP_SPHERE || FADE_PLANE || FADE_SPHERE || CLIP_SPHERES || FADE_SPHERES
	fixed _inverse;
#endif

#if FADE_SPHERE || FADE_PLANE || FADE_SPHERES
	#if !DISSOLVE_GLOW && !SCREENDISSOLVE_GLOW
			uniform sampler2D _TransitionGradient;
	#endif
	uniform fixed _spread = 1;
#endif
#if DISSOLVE || DISSOLVE_GLOW
	#if NOISETRIPLANAR
	uniform sampler2D _Noise2D;
	float tex2DworldMap(float3 co, float3 norm, sampler2D _noise, float _noiseScale)
	 {
		co *= _noiseScale;
		float2 UV;
		float outVal;
		if(abs(norm.x)>abs(norm.y)&&abs(norm.x)>abs(norm.z))
		{
			UV = co.yz; // side
			outVal = tex2D(_noise, UV).r; // use WALLSIDE texture
		}
		else if(abs(norm.z)>abs(norm.y)&&abs(norm.z)>abs(norm.x))
		{
			UV = co.xy; // front
			outVal = tex2D(_noise, UV).r; // use WALL texture
		}
		else
		{
			UV = co.xz; // top
			outVal = tex2D(_noise, UV).r; // use FLR texture
		}
		 return outVal;
	 }
	/*#else
	uniform sampler2D _NoiseAtlas;
	uniform float _atlasSize;
	float texAtlas(float3 co, sampler2D _noise, float _noiseScale)
	 {
		 co *= _noiseScale;
		 float x = frac(co.x);
		 float y = frac(co.y);
		 float z = frac(co.z);

		 float nr = floor(z * _atlasSize * _atlasSize);
		 float column = _atlasSize*frac(nr/_atlasSize);//fmod(nr,  _atlasSize);
		 float row = (nr - column)/_atlasSize;

		 float x1 = (column + x)/_atlasSize;
		 float y1 = (row + y)/_atlasSize;

		 fixed4 col = tex2D(_noise, float2(x1,y1));

		 return col.r;
	 }*/
	 #else
	 uniform sampler3D _Noise3D;
	//uniform float4 _NoiseAtlas_TexelSize;
	#endif
	uniform float _Noise3dScale;

#endif
#if SCREENDISSOLVE || SCREENDISSOLVE_GLOW
	uniform sampler2D _ScreenNoise;
	uniform float4 _ScreenNoise_TexelSize;
	uniform float _ScreenNoiseScale;
#endif
#if CLIP_TUBES
	//I couldn't get arrays to work here;
	uniform float4 _AxisDirs[64];
#endif
#if CLIP_TUBES	|| FADE_SPHERES
	uniform float4 _centerPoints[64];
	uniform float _Radiuses[64];
	uniform int _centerCount = 0;
#endif

	//discard drawing of a point in the world if it is behind any one of the planes.
	bool Clip(float3 posWorld
	//void PlaneClip(float3 posWorld 
		#if NOISETRIPLANAR
		, float3 worldNorm 
		#endif
		) {
		bool _clip = false;
		#if CLIP_TWO_PLANES
		float3 vcross = cross(_SectionPlane,_SectionPlane2);
		if(vcross.y>=0){//<180
			_clip = _clip||(- dot((posWorld - _SectionPoint),_SectionPlane)<0);
			_clip = _clip||(- dot((posWorld - _SectionPoint),_SectionPlane2)<0);
			//if(_clip) discard;
		}
		if(vcross.y<0){//>180
			_clip = _clip || ((_SectionOffset - dot((posWorld - _SectionPoint), _SectionPlane) < 0) && (-dot((posWorld - _SectionPoint), _SectionPlane2) < 0));
		}
		//#else //
		#endif
		#if CLIP_PLANE
			_clip = _clip || ((_SectionOffset - dot((posWorld - _SectionPoint),_SectionPlane))*(1-2*_inverse)<0);
		#endif
		#if CLIP_SPHERE
			_clip = _clip || ((1-2*_inverse)*(dot((posWorld - _SectionPoint),(posWorld - _SectionPoint)) - _Radius*_Radius)<0); //_inverse = 1 : negative to clip the outside of the sphere
		#endif
		#if (FADE_PLANE || FADE_SPHERE || FADE_SPHERES)&&(DISSOLVE || DISSOLVE_GLOW)
			float transparency = 2;
			#if FADE_PLANE
				float dist = -dot((posWorld - _SectionPoint),_SectionPlane);//*(1-2*_inverse);
				transparency = (1/_spread*dist + 0.5);
			#endif
			#if FADE_SPHERE
				float dist = length(posWorld - _SectionPoint);
				transparency = (dist/_spread + 0.5 - _Radius/_spread);//*(1-2*_inverse);
			#endif
			#if FADE_SPHERES
				int _centerCountTruncated = min(_centerCount, 64);
				for (int i = 0; i < _centerCountTruncated; i++)
				{
					float dist = length(posWorld - _centerPoints[i]);
					float _transparency = (dist / _spread + 0.5 - _Radiuses[i] / _spread);//*(1-2*_inverse);
					transparency = min(transparency, _transparency);
				}
			#endif
			//if(transparency>0&&transparency<1)
			//{
			#if DISSOLVE
				float4 col = tex2D(_TransitionGradient, float2(transparency,1));
				if(transparency>=0&&transparency<=1) transparency = col.r;
			#endif
			//}
			#if DISSOLVE || DISSOLVE_GLOW
			#if NOISETRIPLANAR
			fixed4 fade = tex2DworldMap(posWorld, worldNorm, _Noise2D, _Noise3dScale);
			#else
			//fixed4 fade = texAtlas(posWorld, _NoiseAtlas, _Noise3dScale); /////
			fixed4 fade = tex3D(_Noise3D, posWorld* _Noise3dScale);
			#endif
			_clip = _clip || ((fade.r>transparency)&&(_inverse==0));
			_clip = _clip || ((fade.r<=transparency)&&(_inverse==1));
			#endif
		#endif



		#if CLIP_CUBE
		//if(_SectionOffset - dot((posWorld - _SectionPoint),_SectionPlane)<0) discard;
		//if(frac((posWorld - _SectionPoint),_SectionPlane) - 0.5>0) discard;
		fixed _sign = 1-2*_inverse;
		bool _clipCube = (_SectionOffset - dot((posWorld - _SectionPoint - _Radius * _SectionPlane), -_SectionPlane)*_sign < 0) && (_SectionOffset - dot((posWorld - _SectionPoint + _Radius * _SectionPlane), -_SectionPlane)*_sign > 0)
			&& (_SectionOffset - dot((posWorld - _SectionPoint - _Radius * _SectionPlane2), -_SectionPlane2)*_sign < 0) && (_SectionOffset - dot((posWorld - _SectionPoint + _Radius * _SectionPlane2), -_SectionPlane2)*_sign > 0)
			&& (_SectionOffset - dot((posWorld - _SectionPoint - _Radius * _SectionPlane3), -_SectionPlane3)*_sign < 0) && (_SectionOffset - dot((posWorld - _SectionPoint + _Radius * _SectionPlane3), -_SectionPlane3)*_sign > 0);
		//discard;
		//if((_SectionOffset - dot((posWorld - _SectionPoint -_Radius*_SectionPlane2),-_SectionPlane2)<0)&&(_SectionOffset - dot((posWorld - _SectionPoint +_Radius*_SectionPlane2),-_SectionPlane2)>0)) discard;
		#endif


#if CLIP_TUBES
//float3 posRel = posWorld - _SectionPoint;
//float3 posCylinderRel = posRel - _AxisDir * dot(_AxisDir, posRel);
//if ((dot(posCylinderRel,posCylinderRel) - _Radius*_Radius)<0) discard;
		bool _clipTubes = false;
		int _centerCountTruncated = min(_centerCount, 64);
		for (int i = 0; i < _centerCountTruncated; i++)
		{
			_clipTubes = _clipTubes || ((dot(posWorld - _centerPoints[i] - _AxisDirs[i] * dot(_AxisDirs[i], posWorld - _centerPoints[i]), posWorld - _centerPoints[i] - _AxisDirs[i] * dot(_AxisDirs[i], posWorld - _centerPoints[i])) - _Radiuses[i] * _Radiuses[i]) < 0);
		}

		//}
		if (_inverse == 0)
		{
			//if(_clip) discard;
			_clip = _clip || _clipTubes;
		}
		else
		{
			//if(!_clip) discard;
			_clip = _clip || !_clipTubes;
		}
#endif

		//if(_clip) discard;
		return _clip;

	}

#if NOISETRIPLANAR
	void PlaneClip(float3 posWorld, float3 worldNorm) 
	{
		if (Clip(posWorld, worldNorm)) discard;
	}
#else
	void PlaneClip(float3 posWorld)
	{
		if (Clip(posWorld)) discard;
	}
#endif

#if RINGS
	uniform float _timeScale;
	uniform float _n_rings;
	uniform float _animateRings;
	#define M_PI 3.1415926535897932384626433832795
#endif

	#if FADE_PLANE || FADE_SPHERE || FADE_SPHERES
		inline float4 fadeTransition(float3 posWorld
		#if NOISETRIPLANAR
		, float3 worldNorm 
		#endif
		)
		{
			float transparency = 2;
			#if FADE_PLANE&&!FADE_SPHERE
			float dist = -dot((posWorld - _SectionPoint),_SectionPlane);//*(1-2*_inverse);
			transparency = (dist/_spread + 0.5);
			#endif
			#if FADE_SPHERE&&!FADE_PLANE
			float dist = length(posWorld - _SectionPoint);
			transparency = (dist/_spread + 0.5 - _Radius/_spread);//*(1-2*_inverse);
			#endif
		#if RINGS
			float _ringOffset = 0;
			if (_animateRings == 1)
			{
				_ringOffset = frac(_timeScale*(0.5 + _n_rings)*_Time.y) / (0.5 + _n_rings);
			}
			#if FADE_PLANE || FADE_SPHERE
			transparency = transparency + transparency * 0.5*(1 - cos(2 * M_PI*(transparency - _ringOffset) * (0.5 + _n_rings)));
			#endif
		#endif


		#if FADE_SPHERES
			transparency = 1;
			int _centerCountTruncated = min(_centerCount, 64);
			for (int i = 0; i < _centerCountTruncated; i++)
			{
				float dist = length(posWorld - _centerPoints[i]);
				float _transparency = (dist / _spread + 0.5 - _Radiuses[i] / _spread);
				#if RIPPLES
				float4 col = tex2D(_TransitionGradient, float2(_transparency, 1));
				float gradtransparency = col.r/ (_Radiuses[i]+ _spread);
				#endif
			#if RINGS
				#if RIPPLES
				_transparency = 1 - gradtransparency * 0.5*(1 - cos(2 * M_PI*(_transparency - _ringOffset) * (0.5 + _n_rings)));
				#else
				_transparency = _transparency + _transparency * 0.5*(1 - cos(2 * M_PI*(_transparency - _ringOffset) * (0.5 + _n_rings)));
				#endif
			#endif
				_transparency = clamp(_transparency, 0, 1);
				transparency *= _transparency;
			}
			#if !RIPPLES
			float4 col = tex2D(_TransitionGradient, float2(transparency, 1));
			transparency = col.r;
			#endif
		#endif

			float4 rgbcol = float4(0,0,0,0);

			#if !DISSOLVE_GLOW && !SCREENDISSOLVE_GLOW && !FADE_SPHERES
				rgbcol = tex2D(_TransitionGradient, float2(transparency,0));
				float4 col = tex2D(_TransitionGradient, float2(transparency,1));
				transparency = col.r;
			#endif

			#if DISSOLVE_GLOW
				 transparency = (1-2*_inverse)*(transparency - 
				 #if NOISETRIPLANAR
				 tex2DworldMap(posWorld, worldNorm, _Noise2D, _Noise3dScale));
				 #else
				 //texAtlas(posWorld, _NoiseAtlas, _Noise3dScale));
				 tex3D(_Noise3D, posWorld * _Noise3dScale).r);
				 #endif
			#endif

			rgbcol.a = transparency;

			return rgbcol;

		}
		#if NOISETRIPLANAR
		#define WORLD_FADE(posWorld, normWorld) fadeTransition(posWorld, normWorld);
		#else
		#define PLANE_FADE(posWorld) fadeTransition(posWorld);
		#endif
	#endif

//preprocessor macro that will produce an empty block if no clipping planes are used.
		#if NOISETRIPLANAR
		#define WORLD_CLIP(posWorld, normWorld) PlaneClip(posWorld, normWorld);
		#define OUT_MASKED(posWorld, normWorld) Clip(posWorld, normWorld);
		#else
		#define PLANE_CLIP(posWorld) PlaneClip(posWorld);
		#define OUT_MASKED(posWorld) Clip(posWorld);
    #endif
#else
//empty definition
#define PLANE_CLIP(s)

//#define PLANE_FADE(s)
#endif


#endif // PLANE_CLIPPING_INCLUDED