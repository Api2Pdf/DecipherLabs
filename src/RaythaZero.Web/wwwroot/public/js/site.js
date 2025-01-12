// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

import { Application } from "https://unpkg.com/@hotwired/stimulus/dist/stimulus.js";
import FileUploadController from "../controllers/file_upload_controller.js";

window.Stimulus = Application.start();
Stimulus.register("file-upload", FileUploadController);
