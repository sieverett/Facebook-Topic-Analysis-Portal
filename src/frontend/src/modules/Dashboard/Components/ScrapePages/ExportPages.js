import React from 'react';
import Panel from '../Common/Panel';
import DateRangeForm from '../Common/DateRangeForm';

export default function ExportPages(props) {
  return (
    <Panel title="Export">
      <DateRangeForm action="Browse" onSubmit={props.onSubmit}
                     extraButtonAction="Export to CSV" onExtraButtonClicked={props.onExport}
                     lowerName="From" upperName="To" allowEmpty={true} />
    </Panel>
  );
}
