/**
 * $Id: MultiUpload.as 617 2008-11-27 15:48:31Z spocke $
 *
 * @author Moxiecode
 * @copyright Copyright © 2007, Moxiecode Systems AB, All rights reserved.
 */

package com.moxiecode.multiupload {
	import flash.display.LoaderInfo;
	import flash.display.Sprite;
	import flash.errors.IOError;
	import flash.net.FileReferenceList;
	import flash.net.FileReference;
	import flash.net.FileFilter;
	import flash.net.URLLoader;
	import flash.net.URLRequest;
	import flash.net.URLRequestMethod;
	import flash.net.URLVariables;
	import flash.net.URLStream;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.FocusEvent;
	import flash.events.ProgressEvent;
	import flash.events.IOErrorEvent;
	import flash.events.SecurityErrorEvent;
	import flash.events.DataEvent;
	import flash.display.MovieClip;
	import flash.display.StageAlign;
	import flash.display.StageScaleMode;
	import flash.external.ExternalInterface;
	import flash.utils.ByteArray;
	import flash.utils.Dictionary;

	/**
	 * This is the main class of the MultiUpload package.
	 */
	public class MultiUpload extends Sprite {
		// Private fields
		private var clickArea:MovieClip;
		private var fileRefList:FileReferenceList;
		private var files:Dictionary;
		private var idCounter:int = 0;
		private var currentFile:File;
		private var fileFilter:String, cookie:String;

		/**
		 * Main constructor for the MultiUpload class.
		 */
		public function MultiUpload():void {
			if (stage)
				init();
			else
				addEventListener(Event.ADDED_TO_STAGE, init);
		}

		/**
		 * Initialization event handler.
		 *
		 * @param e Event object.
		 */
		private function init(e:Event = null):void {
			removeEventListener(Event.ADDED_TO_STAGE, init);

			// Setup file filter
			this.fileFilter = this.stage.loaderInfo.parameters["file_filter"];
			this.cookie = this.stage.loaderInfo.parameters["cookie"];

			// Setup file reference list
			this.fileRefList = new FileReferenceList();
			this.fileRefList.addEventListener(Event.CANCEL, cancelEvent);
			this.fileRefList.addEventListener(Event.SELECT, selectEvent);
			this.files = new Dictionary();

			// Align and scale stage
			this.stage.align = StageAlign.TOP_LEFT;
			this.stage.scaleMode = StageScaleMode.NO_SCALE;

			// Add something to click on
			this.clickArea = new MovieClip();
			this.clickArea.graphics.beginFill(0x000000, 0); // Fill with transparent color
			this.clickArea.graphics.drawRect(0, 0, 1024, 1024);
			this.clickArea.x = 0;
			this.clickArea.y = 0;
			this.clickArea.width = 1024;
			this.clickArea.height = 1024;
			this.clickArea.graphics.endFill();
			this.clickArea.buttonMode = true;
			this.clickArea.useHandCursor = true;
			addChild(this.clickArea);

			// Register event handlers
			this.clickArea.addEventListener(MouseEvent.ROLL_OVER, this.stageEvent);
			this.clickArea.addEventListener(MouseEvent.ROLL_OUT, this.stageEvent);
			this.clickArea.addEventListener(MouseEvent.CLICK, this.stageClickEvent);
			this.clickArea.addEventListener(MouseEvent.MOUSE_DOWN, this.stageEvent);
			this.clickArea.addEventListener(MouseEvent.MOUSE_UP, this.stageEvent);
			this.clickArea.addEventListener(FocusEvent.FOCUS_IN, this.stageEvent);
			this.clickArea.addEventListener(FocusEvent.FOCUS_OUT, this.stageEvent);

			// Add external callbacks
			ExternalInterface.addCallback('uploadFile', this.uploadFile);
			ExternalInterface.addCallback('removeFile', this.removeFile);
			ExternalInterface.addCallback('cancelUpload', this.cancelUpload);
			ExternalInterface.addCallback('clearQueue', this.clearFiles);

			this.fireEvent("flashInit");
		}

		/**
		 * Event handler for selection cancelled. This simply fires the event out to the page level JS.
		 *
		 * @param e Event object.
		 */
		private function cancelEvent(e:Event):void {
			this.fireEvent("flashCancelSelect");
		}

		/**
		 * Event handler for when the user select files to upload. This method builds up a simpler object
		 * representation and passes this back to the page level JS.
		 *
		 * @param e Event object.
		 */
		private function selectEvent(e:Event):void {
			var selectedFiles:Array = [];

			for (var i:Number = 0; i < this.fileRefList.fileList.length; i++) {
				var file:File = new File("file_" + (this.idCounter++), this.fileRefList.fileList[i]);

				// Add progress listener
				file.addEventListener(ProgressEvent.PROGRESS, function(e:ProgressEvent):void {
					var file:File = e.target as File;

					fireEvent("flashUploadProcess", {
						id : file.id,
						loaded : e.bytesLoaded
					});
				});
	
				// Add error listener
				file.addEventListener(IOErrorEvent.IO_ERROR, function(e:IOErrorEvent):void {
					var file:File = e.target as File;

					fireEvent("flashIOError", {
						id : file.id,
						message : e.text
					});
				});

				// Add error listener
				file.addEventListener(SecurityErrorEvent.SECURITY_ERROR, function(e:SecurityErrorEvent):void {
					var file:File = e.target as File;

					fireEvent("flashSecurityError", {
						id : file.id,
						message : e.text
					});
				});

				// Add response listener
				file.addEventListener(DataEvent.UPLOAD_COMPLETE_DATA, function(e:DataEvent):void {
					var file:File = e.target as File;

					fireEvent("flashUploadComplete", {
						id : file.id,
						text : e.text
					});
				});

				// Add chunk response listener
				file.addEventListener(UploadChunkEvent.UPLOAD_CHUNK_COMPLETE_DATA, function(e:UploadChunkEvent):void {
					var file:File = e.target as File;

					fireEvent("flashUploadChunkComplete", {
						id : file.id,
						text : e.text,
						chunk : e.chunk,
						chunks : e.chunks
					});
				});

				this.files[file.id] = file;

				// Setup selected files object to pass page to page level js
				selectedFiles.push({id : file.id, name : file.fileName, size : file.size, loaded : 0});
			}

			this.fireEvent("flashSelectFiles", selectedFiles);
		}

		/**
		 * Send out all stage events to page level JS inorder to fake click, hover etc.
		 *
		 * @param e Event object.
		 */
		private function stageEvent(e:Event):void {
			this.fireEvent("flashStageEvent:" + e.type);
		}

		/**
		 * Event handler that get executed when the user clicks the state. This will bring up
		 * the file browser dialog.
		 *
		 * @param e Event object.
		 */
		private function stageClickEvent(e:Event):void {
			if (this.fileFilter != null)
				this.fileRefList.browse([new FileFilter("Files", this.fileFilter)]);
			else
				this.fileRefList.browse();
		}

		/**
		 * External interface function. This can be called from page level JS to start the upload of a specific file.
		 *
		 * Settings:
		 * upload_url - Upload URL, where to send the file.
		 * chunk_size - Chunk size, defaults to 1 MB.
		 * post_args - Name/Value object with post arguments to send in on a single chunk upload.
		 *
		 * @param id File id to upload.
		 * @param settings Settings object with name/value items to use with upload such as upload_url, path, file_name and chunksize.
		 */
		private function uploadFile(id:String, settings:Object):void {
			var file:File = this.files[id] as File;

			if (file) {
				// Default chunk size
				if (settings.chunk_size == null)
					settings.chunk_size = 1024 * 1024; // 1 MB

				this.currentFile = file;

				// Upload the file
				file.upload(settings.upload_url, settings.post_args, settings.file_field, new Number(settings.chunk_size), this.cookie);
			}
		}

		/**
		 * Cancels uploading of all files.
		 */
		private function cancelUpload():void {
			if (this.currentFile != null)
				this.currentFile.cancelUpload();
		}

		/**
		 * File id to remove form upload queue.
		 *
		 * @param id Id of the file to remove.
		 */
		private function removeFile(id:String):void {
			if (this.files[id] != null)
				delete this.files[id];
		}

		/**
		 * Remove all files from upload queue.
		 *
		 * @param id Id of the file to remove.
		 */
		private function clearFiles():void {
			this.files = new Dictionary();
		}

		/**
		 * Fires an event from the flash movie out to the page level JS.
		 *
		 * @param type Name of event to fire.
		 * @param obj Object with optional data.
		 */
		private function fireEvent(type:String, obj:Object = null):void {
			ExternalInterface.call("jQuery.multiUpload._fireEvent", type, obj);
		}

		/**
		 * Debugs out a message to Firebug.
		 *
		 * @param msg Message to output to firebug.
		 */
		public static function debug(msg:String):void {
			ExternalInterface.call("console.log", msg);
		}
	}
}
