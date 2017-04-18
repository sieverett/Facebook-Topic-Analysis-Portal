import React, { Component } from 'react';
import DataTable from './Common/Data/DataTable';
import Modal from './Common/Modal';
import Panel from './Common/Panel';
import SubmitButton from './Common/SubmitButton';

class ImportPagesForm extends Component {
  state = {modalId: 'import-pages-modal'}

  componentWillReceiveProps(nextProps) {
    // The user can clear the values of the inputs to display.
    if (nextProps.clear) {
      this.setState({pages: null});
      this.refs.file.value = '';
      nextProps.onClear();
    }
  }

  handleFilesChanged = (event) => {
    // Read the list of files, parse them into JSON and then display them.
    // The input must be an array of objects with "name" and "facebookId".
    if (event.target.files.length > 0) {
      var reader = new FileReader();
      reader.onload = event => {
        var text = event.target.result;
        var result = JSON.parse(text);

        this.setState({pages: result});
      };
      reader.readAsText(event.target.files[0]);
    }
  }

  handleSubmit = (event) => {
    event.preventDefault();

    let errorMessage;
    if (!this.state.pages) {
      errorMessage = <p>Could not read pages from the file.</p>;
    } else if (this.state.pages.length === 0) {
      errorMessage = <p>Pages were empty</p>;
    }

    if (!errorMessage) {
      this.props.onSubmit(this.state.pages);
      return true;
    }

    this.setState({'errorMessage': errorMessage});
    window.showModal('#' + this.state.modalId);
  }

  render() {
    const pageMapping = [
      { name: 'Name', key: path => path.name       },
      { name: 'ID',   key: path => path.facebookId }
    ];

    return (
      <Panel title="Import Pages">
        <form onSubmit={this.handleSubmit}>
          <div className="form-group well">
            <input ref="file" type="file" onChange={this.handleFilesChanged} />
          </div>
          <div className="form-group">
            {this.state.pages &&
              <DataTable showIndex={false} mapping={pageMapping} data={this.state.pages} className='table-bordered' />
            }
          </div>
          <SubmitButton title="Import" />
        </form>
        <Modal id={this.state.modalId} title="Cannot add page">{this.state.errorMessage}</Modal>
      </Panel>
    );
  }
}

export default ImportPagesForm;
