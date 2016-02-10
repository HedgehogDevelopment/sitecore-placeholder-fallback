# Sitecore Placeholder Fallback

## Overview 

This code is documented in this [blog post](http://www.hhogdev.com/blog/2015/september/sitecore-placeholder-fallback.aspx) by Hedgehog Development's Charles Turano. 

When implementing Sitecore websites, sometimes situations arise where the Content Editor wants to personalize or A/B test components that are common to multiple pages in the site. A good example of this is the header and/or navigation components.

The problem is that the components need to behave the same on all pages and it would be very difficult, if not impossible for content editors to maintain these components on all pages on the site.

Placeholder fallback uses the Sitecore Item hierarchy to implement parent fallback for the contents of a placeholder. This will allow personalization and A/B testing using the out of the box components. As an added benefit, the content editor can customize the renderings exactly like all other items on the site. The customizations will automatically be used on the child pages of the site because the child pages fall back to whatever the parent rendering does.

## Installation

[![Build status](https://ci.appveyor.com/api/projects/status/tk4434n23xeri227/branch/master?svg=true)](https://ci.appveyor.com/project/SeanKearney/sitecore-placeholder-fallback/branch/master)

Get the latest version from [GitHub](https://github.com/HedgehogDevelopment/sitecore-placeholder-fallback/releases) or patch releases from [AppVeyor](https://ci.appveyor.com/project/SeanKearney/sitecore-placeholder-fallback/branch/master). Install using Sitecore's Update Installation Wizard.


