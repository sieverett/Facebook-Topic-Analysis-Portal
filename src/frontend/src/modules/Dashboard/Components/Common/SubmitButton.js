import React, { Component } from 'react';

class SubmitButton extends Component {
  render() {
    return (
      <div className="form-group">
        <input type="submit" className="btn btn-primary" value={this.props.title} />
      </div>
    );
  }
}

export default SubmitButton;
