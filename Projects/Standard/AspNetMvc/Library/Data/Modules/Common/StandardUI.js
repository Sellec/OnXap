var StandardUI =
/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 0);
/******/ })
/************************************************************************/
/******/ ({

/***/ "./Modules/Common/SourceJs/StandardUI.js":
/*!***********************************************!*\
  !*** ./Modules/Common/SourceJs/StandardUI.js ***!
  \***********************************************/
/*! exports provided: MainHeader, PrimeVueDataTableFieldFilter, PrimeVueDataTableSourceRequest */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"MainHeader\", function() { return MainHeader; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"PrimeVueDataTableFieldFilter\", function() { return PrimeVueDataTableFieldFilter; });\n/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, \"PrimeVueDataTableSourceRequest\", function() { return PrimeVueDataTableSourceRequest; });\nPrimeVueLibrary.registerGlobalTags();\nconst MainHeader = new Vue({\n  el: \"#js-main-header\",\n  data: {\n    user: {\n      logged: false,\n      idImage: null,\n      access: {\n        controlPanel: false\n      }\n    },\n    ui: {\n      urls: {\n        userSigninUrl: '',\n        userSignoutUrl: '',\n        userProfileUrl: '',\n        userImageUrlTemplate: '',\n        controlPanelUrl: ''\n      }\n    }\n  },\n  methods: {\n    userUpdate: function (val) {\n      this.user.logged = val && val.isGuest ? false : true;\n      this.user.idImage = val && val.idImage ? Number(val.idImage) : null;\n      this.user.access.controlPanel = val && val.access && val.access.controlPanel ? Boolean(val.access.controlPanel) : false;\n    },\n    onRoute: function (nameUrl) {\n      if (!this.ui.urls[nameUrl]) {\n        alert('Некорректный адрес перенаправления!');\n        return;\n      }\n\n      window.location = this.ui.urls[nameUrl];\n    },\n    OnToggleUserMenu: function (event) {\n      this.$refs.userMenu.toggle(event);\n    }\n  },\n  computed: {\n    userImageUrl: function () {\n      return this.user.idImage ? this.ui.urls.userImageUrlTemplate.replace('1234567890', this.user.idImage) : \"/data/img/files/user_noimage.jpg\";\n    },\n    userMenuItems: function () {\n      var items = new Array();\n\n      if (this.user.logged) {\n        items[items.length] = {\n          label: 'Личный кабинет',\n          icon: 'pi pi-user',\n          url: this.ui.urls.userProfileUrl\n        };\n        if (this.user.access.controlPanel) items[items.length] = {\n          label: 'Панель управления',\n          icon: 'pi pi-cog',\n          url: this.ui.urls.controlPanelUrl,\n          target: '_blank'\n        };\n        items[items.length] = {\n          label: 'Выход',\n          icon: 'pi pi-power-off',\n          url: this.ui.urls.userSignoutUrl\n        };\n      }\n\n      return items;\n    }\n  },\n  mounted: function () {\n    this.ui.urls.userSigninUrl = this.$el.getAttribute(\"data-user-signin-url\");\n    this.ui.urls.userSignoutUrl = this.$el.getAttribute(\"data-user-signout-url\");\n    this.ui.urls.userProfileUrl = this.$el.getAttribute(\"data-user-profile-url\");\n    this.ui.urls.userImageUrlTemplate = this.$el.getAttribute(\"data-user-image-template-url\");\n    this.ui.urls.controlPanelUrl = this.$el.getAttribute(\"data-controlpanel-url\");\n  }\n});\nclass PrimeVueDataTableFieldFilter {\n  constructor(source) {\n    this.FieldName = source.field;\n    this.MatchType = source.filterMatchMode === \"contains\" ? 2 : source.filterMatchMode === \"startsWith\" ? 1 : 0;\n    this.Value = source.value;\n  }\n\n}\nclass PrimeVueDataTableSourceRequest {\n  constructor(fieldNameMapper) {\n    this.FirstRow = 0;\n    this.RowsLimit = 0;\n    this.SortByFieldName = null;\n    this.SortByAcsending = true;\n    this.FilterFields = [];\n    this.fieldNameMapper = typeof fieldNameMapper === 'function' ? fieldNameMapper : val => val;\n  }\n\n  ApplyPagination(source) {\n    this.FirstRow = source && source.first ? Number(source.first) : 0;\n    this.RowsLimit = source && source.rows ? Number(source.rows) : 0;\n  }\n\n  ApplySort(source) {\n    this.SortByFieldName = source && source.sortField ? this.fieldNameMapper(String(source.sortField)) : null;\n    this.SortByAcsending = !source ? true : source.sortOrder === 1 ? true : false;\n  }\n\n  ApplyFilter(source) {\n    this.FilterFields = [];\n\n    for (var field in source) {\n      if (source[field]) {\n        this.FilterFields[this.FilterFields.length] = new PrimeVueDataTableFieldFilter({\n          field: this.fieldNameMapper(field),\n          value: source[field],\n          filterMatchMode: 'contains'\n        });\n      }\n    }\n  }\n\n}\n\n//# sourceURL=webpack://StandardUI/./Modules/Common/SourceJs/StandardUI.js?");

/***/ }),

/***/ 0:
/*!*****************************************************!*\
  !*** multi ./Modules/Common/SourceJs/StandardUI.js ***!
  \*****************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

eval("module.exports = __webpack_require__(/*! ./Modules\\Common\\SourceJs\\StandardUI.js */\"./Modules/Common/SourceJs/StandardUI.js\");\n\n\n//# sourceURL=webpack://StandardUI/multi_./Modules/Common/SourceJs/StandardUI.js?");

/***/ })

/******/ });