import { Controller } from "https://unpkg.com/@hotwired/stimulus/dist/stimulus.js";
import { Uppy, Dashboard, XHRUpload } from "https://releases.transloadit.com/uppy/v4.9.0/uppy.min.mjs"
export default class extends Controller {
    static targets = ["uploadContainer"];
    
    static values = {
        maxFiles: Number,
        csrfToken: String,
        successEndpoint: String
    }
    connect() {
        let uppy;

        if (uppy) {
            uppy.close();
        }

        uppy = new Uppy({
            restrictions: { maxNumberOfFiles: this.maxFilesValue },
            autoProceed: true
        });

        uppy.use(Dashboard, {
            target: this.uploadContainerTarget,
            inline: true,
            showProgressDetails: true,
            height: 300,
            width: 900,
        });

        uppy.use(XHRUpload, {
            endpoint: '/admin/media-items/upload',
            fieldName: 'file',
            headers: {
                'RequestVerificationToken': this.csrfTokenValue
            },
            formData: true,
        });

        uppy.on('complete', (result) => {
            console.log('successful files:', result.successful);
            console.log('failed files:', result.failed);

            const uploadedIds = [];

            result.successful.forEach(file => {
                if (file.response && file.response.body && file.response.body.fields) {
                    const id = file.response.body.fields.id;
                    if (id) {
                        uploadedIds.push(id);
                    }
                }
            });

            console.log('Uploaded IDs:', uploadedIds);

            // Send a Turbo Stream request with the uploadedIds array
            fetch(this.successEndpointValue, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': this.csrfTokenValue,
                    'Content-Type': 'application/json',
                    'Accept': 'text/vnd.turbo-stream.html' // Request Turbo Stream response
                },
                body: JSON.stringify({
                    UploadedIds: uploadedIds,
                    MaxFiles: this.maxFilesValue
                })
            })
                .then(response => response.text())
                .then(html => Turbo.renderStreamMessage(html))
                .catch(error => console.error('Turbo stream error:', error));
        });
    }
}
