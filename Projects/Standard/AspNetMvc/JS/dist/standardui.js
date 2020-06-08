var StandardUI=function(e){var t={};function r(i){if(t[i])return t[i].exports;var n=t[i]={i:i,l:!1,exports:{}};return e[i].call(n.exports,n,n.exports,r),n.l=!0,n.exports}return r.m=e,r.c=t,r.d=function(e,t,i){r.o(e,t)||Object.defineProperty(e,t,{enumerable:!0,get:i})},r.r=function(e){"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},r.t=function(e,t){if(1&t&&(e=r(e)),8&t)return e;if(4&t&&"object"==typeof e&&e&&e.__esModule)return e;var i=Object.create(null);if(r.r(i),Object.defineProperty(i,"default",{enumerable:!0,value:e}),2&t&&"string"!=typeof e)for(var n in e)r.d(i,n,function(t){return e[t]}.bind(null,n));return i},r.n=function(e){var t=e&&e.__esModule?function(){return e.default}:function(){return e};return r.d(t,"a",t),t},r.o=function(e,t){return Object.prototype.hasOwnProperty.call(e,t)},r.p="",r(r.s=0)}([function(e,t,r){"use strict";function i(e,t){for(var r=0;r<t.length;r++){var i=t[r];i.enumerable=i.enumerable||!1,i.configurable=!0,"value"in i&&(i.writable=!0),Object.defineProperty(e,i.key,i)}}function n(e,t){if(!(e instanceof t))throw new TypeError("Cannot call a class as a function")}r.r(t),r.d(t,"MainHeader",(function(){return l})),r.d(t,"PrimeVueDataTableFieldFilter",(function(){return u})),r.d(t,"PrimeVueDataTableSourceRequest",(function(){return s})),PrimeVueLibrary.registerGlobalTags();var l=new Vue({el:"#js-main-header",data:{user:{logged:!1,idImage:null,access:{controlPanel:!1}},ui:{urls:{userSigninUrl:"",userSignoutUrl:"",userProfileUrl:"",userImageUrlTemplate:"",controlPanelUrl:""}}},methods:{userUpdate:function(e){this.user.logged=!e||!e.isGuest,this.user.idImage=e&&e.idImage?Number(e.idImage):null,this.user.access.controlPanel=!!(e&&e.access&&e.access.controlPanel)&&Boolean(e.access.controlPanel)},onRoute:function(e){this.ui.urls[e]?window.location=this.ui.urls[e]:alert("Некорректный адрес перенаправления!")},OnToggleUserMenu:function(e){this.$refs.userMenu.toggle(e)}},computed:{userImageUrl:function(){return this.user.idImage?this.ui.urls.userImageUrlTemplate.replace("1234567890",this.user.idImage):"/data/img/files/user_noimage.jpg"},userMenuItems:function(){var e=new Array;return this.user.logged&&(e[e.length]={label:"Личный кабинет",icon:"pi pi-user",url:this.ui.urls.userProfileUrl},this.user.access.controlPanel&&(e[e.length]={label:"Панель управления",icon:"pi pi-cog",url:this.ui.urls.controlPanelUrl,target:"_blank"}),e[e.length]={label:"Выход",icon:"pi pi-power-off",url:this.ui.urls.userSignoutUrl}),e}},mounted:function(){this.ui.urls.userSigninUrl=this.$el.getAttribute("data-user-signin-url"),this.ui.urls.userSignoutUrl=this.$el.getAttribute("data-user-signout-url"),this.ui.urls.userProfileUrl=this.$el.getAttribute("data-user-profile-url"),this.ui.urls.userImageUrlTemplate=this.$el.getAttribute("data-user-image-template-url"),this.ui.urls.controlPanelUrl=this.$el.getAttribute("data-controlpanel-url")}}),u=function e(t){n(this,e),this.FieldName=t.field,this.MatchType="contains"===t.filterMatchMode?2:"startsWith"===t.filterMatchMode?1:0,this.Value=t.value},s=function(){function e(t){n(this,e),this.FirstRow=0,this.RowsLimit=0,this.SortByFieldName=null,this.SortByAcsending=!0,this.FilterFields=[],this.fieldNameMapper="function"==typeof t?t:function(e){return e}}var t,r,l;return t=e,(r=[{key:"ApplyPagination",value:function(e){this.FirstRow=e&&e.first?Number(e.first):0,this.RowsLimit=e&&e.rows?Number(e.rows):0}},{key:"ApplySort",value:function(e){this.SortByFieldName=e&&e.sortField?this.fieldNameMapper(String(e.sortField)):null,this.SortByAcsending=!e||1===e.sortOrder}},{key:"ApplyFilter",value:function(e){for(var t in this.FilterFields=[],e)e[t]&&(this.FilterFields[this.FilterFields.length]=new u({field:this.fieldNameMapper(t),value:e[t],filterMatchMode:"contains"}))}}])&&i(t.prototype,r),l&&i(t,l),e}()}]);