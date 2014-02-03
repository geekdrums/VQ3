========================================================================
 SpriteStudioPlayer for Unity

  Introduction

 Copyright(C) 2003-2013 Web Technology Corp.
========================================================================

Thank you for choosing SpriteStudioPlayer for Unity.
Please read this document before using SpriteStudioPlayer for Unity.


------------------------------------------------------------------------
* Introduction

  SpriteStudioPlayer for Unity is a plugin to display and control animation data created with SpriteStudio on Unity.

  [IMPORTANT]
  This asset and the input file(ssax) for the asset are currently expired.
  Therefore, there is no plan to improve or add some features in the future.
  Please try to our new asset "SS5Player for Unity(with OPTPiX SpriteStudio)
  if you do not have any reasons to keep using this asset.

  Download SS5Player for Unity from here
  http://www.webtech.co.jp/help/ja/spritestudio/support/tool_sample_download/

------------------------------------------------------------------------
* Use

  Refer to usage.txt indicated in *Contents below.
  Please verify each document.

------------------------------------------------------------------------
* Contents

	+SpriteStudio/
		+Documents/
			+English/
			+Japanese/
				readme.txt: This document.
				history.txt: Revision history.
				usage.txt: Installation and usage.
				sample.txt: Description of the samples.
				script.txt: Reference for the script.

		+Editor/ 	: EditorClass group for this plugin.
		+Runtime/	: RuntimeClass group for this plugin.
		+Shaders/	: Shader group for this plugin.
		+Samples/ 	: Samples.
						  See sample.txt for details.

------------------------------------------------------------------------
* Operating Conditions

  Supported by Unity version 3.5.
  In addition, operations are tested only on the following platforms.

  - Windows
  - Mac
  - WebPlayer
  - iPhone 3GS
  - Android 2.2/3.0

------------------------------------------------------------------------
* Terminology

  SpriteObject refers to a GameObject created to playback SpriteStudio animations, which has SsSprite script attached.

------------------------------------------------------------------------
* SpriteStudio versions

  Data output from SpriteStudio version 4.00.19 or later must be used.
  
  When saving motion data, select "motion text data (*.ssax)" as the file type.
  
  Next, save by checking
  Output data for SpriteStudioPlayer for Unity
  in the Save Option dialog box.
  
  See Animation data import procedure in usage.txt for the steps to import the data created with the above procedure.

------------------------------------------------------------------------
* Checking the installed version

  Select About in the SpriteStudio menu located in the upper part of the Unity main window to check the version.

  SpriteStudioPlayer Version indicates the plugin version.
  Ssax File Version indicates the version of the .ssax file that can be read by the current plugin.

------------------------------------------------------------------------
* Cautions

  "Regarding display of translucent parts that have ColorBlending animation applied"
  
  For translucent parts that have ColorBlending animation applied, opacity is ignored in an OpenGL ES 1.x environment (early model iPhone/Android terminals).

  This is because the OpenGL ES 1.x environment does not support the use of CgProgram to handle ColorBlending intensity parameters and the parts opacity simultaneously.

  - SSAX file format specifications are subject to future change.
  
------------------------------------------------------------------------
* Contact

  salesgrp@webtech.co.jp


============================================================================
Web Technology Corp.
http://www.webtech.co.jp/eng/
Copyright(C) 2003-2013 Web Technology Corp.
============================================================================

* 'SpriteStudio' and 'Web Technology' are registered trademarks of
  Web Technology Corp.
* All other trademarks and registered trademarks are the sole property of
  their respective owners.

[End of TEXT]
