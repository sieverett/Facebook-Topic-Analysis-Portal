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
      this.clear();
      nextProps.onClear();
    }
  }

  clear = () => {
    this.setState({pages: null, displayedErrorMessage: null});
    this.refs.file.value = '';    
  }

  handleFilesChanged = (event) => {
    // Read the list of files, parse them into JSON and then display them.
    // The input must be an array of objects with "name" and "facebookId".
    if (event.target.files.length > 0) {
      const reader = new FileReader();
      reader.onload = event => {
        const text = event.target.result;
        try {
          const pages = JSON.parse(text);
          this.setState({pages, displayedErrorMessage: null});
        }
        catch (e) {
          this.clear();
          this.setState({displayedErrorMessage: e.message});
        }
      };
      reader.readAsText(event.target.files[0]);
    }
  }

  handleSubmit = (event) => {
    event.preventDefault();

    let modalErrorMessage;
    if (!this.state.pages) {
      modalErrorMessage = 'Could not read pages from the file.';
    } else if (this.state.pages.length === 0) {
      modalErrorMessage = 'Pages were empty';
    }

    if (!modalErrorMessage) {
      this.props.onSubmit(this.state.pages);
      return true;
    }

    this.setState({modalErrorMessage});
    window.showModal('#' + this.state.modalId);
  }

  render() {
    const pageMapping = [
      { name: 'Name', key: path => path.name       },
      { name: 'ID',   key: path => path.facebookId }
    ];
    const { pages, displayedErrorMessage, modalId, modalErrorMessage } = this.state;

    return (
      <Panel title="Import Pages">
        <form onSubmit={this.handleSubmit}>
          <div className="form-group well">
            <input ref="file" type="file" onChange={this.handleFilesChanged} />
          </div>
          <div className="form-group">
            {pages &&
              <DataTable showIndex={false} mapping={pageMapping} data={pages} className='table-bordered' />
            }
            {displayedErrorMessage &&
              <p className="text-danger">Error: {displayedErrorMessage}</p>
            }
          </div>
          <SubmitButton title="Import" />
        </form>
        <Modal id={modalId} title="Cannot add page">{modalErrorMessage}</Modal>
      </Panel>
    );
  }
}

export default ImportPagesForm;
